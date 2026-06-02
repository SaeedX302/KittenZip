// KittensHandler.cpp

#include "StdAfx.h"

#include "../../Common/ComTry.h"
#include "../Common/RegisterArc.h"
#include "../Common/StreamUtils.h"
#include "../Common/FileStreams.h"
#include "../../../C/7zCrc.h"

#include "7z/7zHandler.h"

using namespace NWindows;

namespace NArchive {
namespace NKittens {

static const Byte kKittensSecretKey[] = {
  0x4B, 0x49, 0x54, 0x54, 0x45, 0x4E, 0x53, 0x5F, // "KITTENS_"
  0x53, 0x45, 0x43, 0x52, 0x45, 0x54, 0x5F, 0x4B, // "SECRET_K"
  0x45, 0x59, 0x5F, 0x32, 0x30, 0x32, 0x36, 0x5F, // "EY_2026_"
  0x41, 0x44, 0x4D, 0x49, 0x4E, 0x5F, 0x39, 0x39  // "ADMIN_99"
};

static const char* kReadmeContent =
  "This is a .kittens file compressed using KittenZip.\n"
  "To extract this archive, please install KittenZip.\n\n"
  "Credits: Made by TSun & 1Shot (Script Kittens)\n";

// Custom XOR Stream implementing IInStream for seeking and reading decrypted archive payload on the fly
class CXorInStream Z7_final :
  public IInStream,
  public CMyUnknownImp
{
  CMyComPtr<IInStream> _stream;
  const Byte* _key;
  size_t _keyLen;
  UInt64 _startPos;
  UInt64 _curPos;
public:
  CXorInStream(IInStream* stream, const Byte* key, size_t keyLen, UInt64 startPos)
    : _stream(stream), _key(key), _keyLen(keyLen), _startPos(startPos), _curPos(0) {}

  Z7_COM_QI_BEGIN2(IInStream)
  Z7_COM_QI_END
  Z7_COM_ADDREF_RELEASE

  STDMETHOD(Read)(void *data, UInt32 size, UInt32 *processedSize) Z7_override {
    UInt32 realProcessed = 0;
    HRESULT res = _stream->Seek(_startPos + _curPos, STREAM_SEEK_SET, NULL);
    if (res != S_OK) return res;

    res = _stream->Read(data, size, &realProcessed);
    if (res == S_OK || res == S_FALSE) {
      Byte* p = (Byte*)data;
      for (UInt32 i = 0; i < realProcessed; i++) {
        p[i] ^= _key[(_curPos + i) % _keyLen];
      }
      _curPos += realProcessed;
    }
    if (processedSize) *processedSize = realProcessed;
    return res;
  }

  STDMETHOD(Seek)(Int64 offset, UInt32 seekOrigin, UInt64 *newPosition) Z7_override {
    UInt64 nextPos = _curPos;
    if (seekOrigin == STREAM_SEEK_SET) {
      nextPos = offset;
    } else if (seekOrigin == STREAM_SEEK_CUR) {
      nextPos = _curPos + offset;
    } else if (seekOrigin == STREAM_SEEK_END) {
      UInt64 streamSize = 0;
      HRESULT res = _stream->Seek(0, STREAM_SEEK_END, &streamSize);
      if (res != S_OK) return res;
      if (streamSize < _startPos) {
        nextPos = 0;
      } else {
        nextPos = (streamSize - _startPos) + offset;
      }
    }
    _curPos = nextPos;
    if (newPosition) *newPosition = _curPos;
    return S_OK;
  }
};

static void WriteUInt16(ISequentialOutStream* stream, UInt16 val) {
  Byte buf[2];
  buf[0] = (Byte)val;
  buf[1] = (Byte)(val >> 8);
  WriteStream(stream, buf, 2);
}

static void WriteUInt32(ISequentialOutStream* stream, UInt32 val) {
  Byte buf[4];
  buf[0] = (Byte)val;
  buf[1] = (Byte)(val >> 8);
  buf[2] = (Byte)(val >> 16);
  buf[3] = (Byte)(val >> 24);
  WriteStream(stream, buf, 4);
}

static HRESULT WriteKittensArchive(
    ISequentialOutStream* outStream,
    IInStream* tempPayloadStream,
    UInt64 payloadSize,
    UInt32 payloadCrc)
{
  UInt32 readmeLen = (UInt32)strlen(kReadmeContent);
  UInt32 readmeCrc = CrcCalc(kReadmeContent, readmeLen);

  // 1. Local File Header 1 (READ_ME.txt)
  WriteUInt32(outStream, 0x04034b50); // Signature
  WriteUInt16(outStream, 10);         // Version needed
  WriteUInt16(outStream, 0);          // GP flag
  WriteUInt16(outStream, 0);          // Compression method (stored)
  WriteUInt32(outStream, 0);          // Mod time/date
  WriteUInt32(outStream, readmeCrc);  // CRC-32
  WriteUInt32(outStream, readmeLen);  // Compressed size
  WriteUInt32(outStream, readmeLen);  // Uncompressed size
  WriteUInt16(outStream, 11);         // Filename length
  WriteUInt16(outStream, 0);          // Extra field length
  WriteStream(outStream, "READ_ME.txt", 11);
  WriteStream(outStream, kReadmeContent, readmeLen);

  // 2. Local File Header 2 (payload.dat)
  WriteUInt32(outStream, 0x04034b50); // Signature
  WriteUInt16(outStream, 10);         // Version needed
  WriteUInt16(outStream, 0);          // GP flag
  WriteUInt16(outStream, 0);          // Compression method (stored)
  WriteUInt32(outStream, 0);          // Mod time/date
  WriteUInt32(outStream, payloadCrc); // CRC-32
  WriteUInt32(outStream, (UInt32)payloadSize); // Compressed size
  WriteUInt32(outStream, (UInt32)payloadSize); // Uncompressed size
  WriteUInt16(outStream, 11);         // Filename length
  WriteUInt16(outStream, 0);          // Extra field length
  WriteStream(outStream, "payload.dat", 11);

  // 3. Write payload (XOR encrypted)
  RINOK(tempPayloadStream->Seek(0, STREAM_SEEK_SET, NULL));
  Byte buf[4096];
  UInt64 totalWritten = 0;
  while (true) {
    UInt32 read = 0;
    RINOK(tempPayloadStream->Read(buf, 4096, &read));
    if (read == 0) break;
    for (UInt32 i = 0; i < read; i++) {
      buf[i] ^= kKittensSecretKey[(totalWritten + i) % sizeof(kKittensSecretKey)];
    }
    UInt32 written = 0;
    RINOK(outStream->Write(buf, read, &written));
    if (written != read) return E_FAIL;
    totalWritten += read;
  }

  // 4. Central Directory Header 1 (READ_ME.txt)
  UInt32 readmeOffset = 0;
  WriteUInt32(outStream, 0x02014b50); // CD signature
  WriteUInt16(outStream, 20);         // Version made
  WriteUInt16(outStream, 10);         // Version needed
  WriteUInt16(outStream, 0);          // GP flag
  WriteUInt16(outStream, 0);          // Compression method
  WriteUInt32(outStream, 0);          // Mod time/date
  WriteUInt32(outStream, readmeCrc);  // CRC-32
  WriteUInt32(outStream, readmeLen);  // Compressed size
  WriteUInt32(outStream, readmeLen);  // Uncompressed size
  WriteUInt16(outStream, 11);         // Filename length
  WriteUInt16(outStream, 0);          // Extra field length
  WriteUInt16(outStream, 0);          // File comment length
  WriteUInt16(outStream, 0);          // Disk number start
  WriteUInt16(outStream, 0);          // Internal attr
  WriteUInt32(outStream, 0);          // External attr
  WriteUInt32(outStream, readmeOffset); // Local header offset (0)
  WriteStream(outStream, "READ_ME.txt", 11);

  // 5. Central Directory Header 2 (payload.dat)
  UInt32 payloadOffset = 30 + 11 + readmeLen;
  WriteUInt32(outStream, 0x02014b50); // CD signature
  WriteUInt16(outStream, 20);         // Version made
  WriteUInt16(outStream, 10);         // Version needed
  WriteUInt16(outStream, 0);          // GP flag
  WriteUInt16(outStream, 0);          // Compression method
  WriteUInt32(outStream, 0);          // Mod time/date
  WriteUInt32(outStream, payloadCrc); // CRC-32
  WriteUInt32(outStream, (UInt32)payloadSize); // Compressed size
  WriteUInt32(outStream, (UInt32)payloadSize); // Uncompressed size
  WriteUInt16(outStream, 11);         // Filename length
  WriteUInt16(outStream, 0);          // Extra field length
  WriteUInt16(outStream, 0);          // File comment length
  WriteUInt16(outStream, 0);          // Disk number start
  WriteUInt16(outStream, 0);          // Internal attr
  WriteUInt32(outStream, 0);          // External attr
  WriteUInt32(outStream, payloadOffset); // Local header offset
  WriteStream(outStream, "payload.dat", 11);

  // 6. EOCD
  UInt32 cdSize = (46 + 11) + (46 + 11);
  UInt32 cdOffset = payloadOffset + 30 + 11 + (UInt32)payloadSize;
  WriteUInt32(outStream, 0x06054b50); // EOCD signature
  WriteUInt16(outStream, 0);          // Disk number
  WriteUInt16(outStream, 0);          // Disk where CD starts
  WriteUInt16(outStream, 2);          // Disk records
  WriteUInt16(outStream, 2);          // Total records
  WriteUInt32(outStream, cdSize);     // CD size
  WriteUInt32(outStream, cdOffset);   // CD offset
  WriteUInt16(outStream, 0);          // Comment length

  return S_OK;
}

// CKittensHandler implements the Kittens custom archive format handler
class CKittensHandler Z7_final :
  public IInArchive,
  public IOutArchive,
  public ISetProperties,
  public CMyUnknownImp
{
  CMyComPtr<IInStream> _stream;
  CMyComPtr<CXorInStream> _xorInStream;
  CMyComPtr<IUnknown> _innerHandler;
  CMyComPtr<IInArchive> _innerInArchive;

  CObjectVector<UString> _names;
  CObjectVector<NCOM::CPropVariant> _values;

public:
  CKittensHandler() {}
  ~CKittensHandler() { Close(); }

  Z7_COM_QI_BEGIN2(IInArchive)
  Z7_COM_QI_ENTRY(IOutArchive)
  Z7_COM_QI_ENTRY(ISetProperties)
  Z7_COM_QI_END
  Z7_COM_ADDREF_RELEASE

  STDMETHOD(Open)(IInStream *stream, const UInt64 *maxCheckStartPosition, IArchiveOpenCallback *openCallback) Z7_override;
  STDMETHOD(Close)() Z7_override;
  STDMETHOD(GetNumberOfItems)(UInt32 *numItems) Z7_override;
  STDMETHOD(GetProperty)(UInt32 index, PROPID propID, PROPVARIANT *value) Z7_override;
  STDMETHOD(Extract)(const UInt32 *indices, UInt32 numItems, Int32 testMode, IArchiveExtractCallback *extractCallback) Z7_override;

  STDMETHOD(GetArchiveProperty)(PROPID propID, PROPVARIANT *value) Z7_override;
  STDMETHOD(GetNumberOfProperties)(UInt32 *numProperties) Z7_override;
  STDMETHOD(GetPropertyInfo)(UInt32 index, BSTR *name, PROPID *propID, VARTYPE *varType) Z7_override;
  STDMETHOD(GetNumberOfArchiveProperties)(UInt32 *numProperties) Z7_override;
  STDMETHOD(GetArchivePropertyInfo)(UInt32 index, BSTR *name, PROPID *propID, VARTYPE *varType) Z7_override;

  STDMETHOD(UpdateItems)(ISequentialOutStream *outStream, UInt32 numItems, IArchiveUpdateCallback *updateCallback) Z7_override;
  STDMETHOD(GetFileTimeType)(UInt32 *timeType) Z7_override;

  STDMETHOD(SetProperties)(const wchar_t * const *names, const PROPVARIANT *values, UInt32 numProps) Z7_override;
};

Z7_COM7F_IMF(CKittensHandler::Open(IInStream *stream, const UInt64 *maxCheckStartPosition, IArchiveOpenCallback *openCallback))
{
  COM_TRY_BEGIN
  Close();

  // Read the first 1024 bytes to find "payload.dat"
  Byte buf[1024];
  UInt32 processedSize = 0;
  RINOK(stream->Seek(0, STREAM_SEEK_SET, NULL));
  RINOK(stream->Read(buf, 1024, &processedSize));
  if (processedSize < 100) return S_FALSE;

  // Search for "payload.dat" filename in the buffer
  int idx = -1;
  const char* target = "payload.dat";
  size_t targetLen = strlen(target);
  for (UInt32 i = 0; i <= processedSize - targetLen; i++) {
    if (memcmp(buf + i, target, targetLen) == 0) {
      idx = i;
      break;
    }
  }

  if (idx < 30) {
    return S_FALSE; // Not a valid kittens archive
  }

  int L = idx - 30;
  // Check local file header signature 0x04034b50 (PK\x03\x04)
  if (buf[L] != 0x50 || buf[L+1] != 0x4B || buf[L+2] != 0x03 || buf[L+3] != 0x04) {
    return S_FALSE;
  }

  UInt32 N = buf[L+26] | (buf[L+27] << 8);
  UInt32 M = buf[L+28] | (buf[L+29] << 8);
  UInt64 payloadOffset = L + 30 + N + M;

  _stream = stream;
  _xorInStream = new CXorInStream(stream, kKittensSecretKey, sizeof(kKittensSecretKey), payloadOffset);

  NArchive::N7z::CHandler *innerHandlerSpec = new NArchive::N7z::CHandler();
  _innerHandler = innerHandlerSpec;

  HRESULT res = _innerHandler->QueryInterface(IID_IInArchive, (void**)&_innerInArchive);
  if (res != S_OK) return res;

  res = _innerInArchive->Open(_xorInStream, maxCheckStartPosition, openCallback);
  if (res != S_OK) {
    Close();
    return res;
  }

  return S_OK;
  COM_TRY_END
}

Z7_COM7F_IMF(CKittensHandler::Close())
{
  if (_innerInArchive) {
    _innerInArchive->Close();
    _innerInArchive.Release();
  }
  _innerHandler.Release();
  _xorInStream.Release();
  _stream.Release();
  return S_OK;
}

Z7_COM7F_IMF(CKittensHandler::GetNumberOfItems(UInt32 *numItems))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->GetNumberOfItems(numItems);
}

Z7_COM7F_IMF(CKittensHandler::GetProperty(UInt32 index, PROPID propID, PROPVARIANT *value))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->GetProperty(index, propID, value);
}

Z7_COM7F_IMF(CKittensHandler::Extract(const UInt32 *indices, UInt32 numItems, Int32 testMode, IArchiveExtractCallback *extractCallback))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->Extract(indices, numItems, testMode, extractCallback);
}

Z7_COM7F_IMF(CKittensHandler::GetArchiveProperty(PROPID propID, PROPVARIANT *value))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->GetArchiveProperty(propID, value);
}

Z7_COM7F_IMF(CKittensHandler::GetNumberOfProperties(UInt32 *numProperties))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->GetNumberOfProperties(numProperties);
}

Z7_COM7F_IMF(CKittensHandler::GetPropertyInfo(UInt32 index, BSTR *name, PROPID *propID, VARTYPE *varType))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->GetPropertyInfo(index, name, propID, varType);
}

Z7_COM7F_IMF(CKittensHandler::GetNumberOfArchiveProperties(UInt32 *numProperties))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->GetNumberOfArchiveProperties(numProperties);
}

Z7_COM7F_IMF(CKittensHandler::GetArchivePropertyInfo(UInt32 index, BSTR *name, PROPID *propID, VARTYPE *varType))
{
  if (!_innerInArchive) return E_FAIL;
  return _innerInArchive->GetArchivePropertyInfo(index, name, propID, varType);
}

Z7_COM7F_IMF(CKittensHandler::UpdateItems(ISequentialOutStream *outStream, UInt32 numItems, IArchiveUpdateCallback *updateCallback))
{
  COM_TRY_BEGIN
  
  // 1. Create a temporary file path
  WCHAR tempPath[MAX_PATH];
  if (!GetTempPathW(MAX_PATH, tempPath)) return E_FAIL;
  WCHAR tempFileName[MAX_PATH];
  if (!GetTempFileNameW(tempPath, L"kit", 0, tempFileName)) return E_FAIL;

  HRESULT res = S_OK;
  UInt64 payloadSize = 0;
  UInt32 payloadCrc = CRC_INIT_VAL;

  {
    COutFileStream *tempStreamSpec = new COutFileStream();
    CMyComPtr<ISequentialOutStream> tempStream = tempStreamSpec;
    if (!tempStreamSpec->Open(tempFileName, CREATE_ALWAYS)) {
      DeleteFileW(tempFileName);
      return E_FAIL;
    }

    NArchive::N7z::CHandler *innerHandlerSpec = new NArchive::N7z::CHandler();
    CMyComPtr<IUnknown> innerHandler = innerHandlerSpec;

    CMyComPtr<IOutArchive> innerOutArchive;
    res = innerHandler->QueryInterface(Z7_GET_IID_FROM_GUID(IID_IOutArchive), (void**)&innerOutArchive);
    if (res != S_OK) {
      tempStreamSpec->Close();
      DeleteFileW(tempFileName);
      return res;
    }

    if (_names.Size() > 0) {
      CMyComPtr<ISetProperties> setProps;
      innerHandler->QueryInterface(Z7_GET_IID_FROM_GUID(IID_ISetProperties), (void**)&setProps);
      if (setProps) {
        CRecordVector<const wchar_t *> namePtrs;
        CRecordVector<PROPVARIANT> propVars;
        for (unsigned i = 0; i < _names.Size(); i++) {
          namePtrs.Add(_names[i]);
          propVars.Add(_values[i]);
        }
        setProps->SetProperties(&namePtrs[0], &propVars[0], _names.Size());
      }
    }

    res = innerOutArchive->UpdateItems(tempStream, numItems, updateCallback);
    tempStreamSpec->Close();
  }

  if (res != S_OK) {
    DeleteFileW(tempFileName);
    return res;
  }

  // 2. Open temporary file for reading to calculate CRC-32 and write to the output stream
  {
    CInFileStream *tempInStreamSpec = new CInFileStream();
    CMyComPtr<IInStream> tempInStream = tempInStreamSpec;
    if (!tempInStreamSpec->Open(tempFileName)) {
      DeleteFileW(tempFileName);
      return E_FAIL;
    }

    tempInStreamSpec->GetSize(&payloadSize);

    // Calculate CRC-32 of the raw payload
    Byte buf[4096];
    RINOK(tempInStream->Seek(0, STREAM_SEEK_SET, NULL));
    while (true) {
      UInt32 read = 0;
      res = tempInStream->Read(buf, 4096, &read);
      if (res != S_OK && res != S_FALSE) {
        tempInStreamSpec->Close();
        DeleteFileW(tempFileName);
        return res;
      }
      if (read == 0) break;
      payloadCrc = CrcUpdate(payloadCrc, buf, read);
    }
    payloadCrc = CRC_GET_DIGEST(payloadCrc);

    // Write the output Kittens ZIP package
    res = WriteKittensArchive(outStream, tempInStream, payloadSize, payloadCrc);
    
    tempInStreamSpec->Close();
  }

  DeleteFileW(tempFileName);
  return res;
  COM_TRY_END
}

Z7_COM7F_IMF(CKittensHandler::GetFileTimeType(UInt32 *timeType))
{
  *timeType = NFileTimeType::kWindows;
  return S_OK;
}

Z7_COM7F_IMF(CKittensHandler::SetProperties(const wchar_t * const *names, const PROPVARIANT *values, UInt32 numProps))
{
  _names.Clear();
  _values.Clear();
  for (UInt32 i = 0; i < numProps; i++) {
    _names.Add(names[i]);
    _values.Add(values[i]);
  }
  return S_OK;
}

static const Byte k_Signature[] = { 0x50, 0x4B, 0x03, 0x04 };

static API_FUNC_static_IsArc IsArc_Kittens(const Byte *p, size_t size)
{
  if (size < 4) return k_IsArc_Res_NEED_MORE;
  if (p[0] == 0x50 && p[1] == 0x4B && p[2] == 0x03 && p[3] == 0x04) {
    return k_IsArc_Res_YES;
  }
  return k_IsArc_Res_NO;
}

REGISTER_ARC_I_CLS(
  CKittensHandler,
  "kittens", "kittens", NULL, 0x88,
  k_Signature,
  0,
  NArcInfoFlags::kFindSignature
  | NArcInfoFlags::kCTime
  | NArcInfoFlags::kATime
  | NArcInfoFlags::kMTime
  | NArcInfoFlags::kMTime_Default
  , IsArc_Kittens)

}}
