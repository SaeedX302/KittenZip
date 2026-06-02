/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.LZ5.cpp
 * PURPOSE:    Implementation for LZ5 Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#include "KittenZip.Codecs.MultiThreadWrapper.LZ5.h"

#include <cstddef>

EXTERN_C int KittenZipCodecsLz5Read(
    void* Context,
    LZ5MT_Buffer* Input)
{
    KittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT ConvertedInput;
    ConvertedInput.Buffer = Input->buf;
    ConvertedInput.Size = Input->size;
    ConvertedInput.Allocated = Input->allocated;
    int Result = ::KittenZipCodecsCommonRead(
        reinterpret_cast<PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT>(Context),
        &ConvertedInput);
    Input->buf = ConvertedInput.Buffer;
    Input->size = ConvertedInput.Size;
    Input->allocated = ConvertedInput.Allocated;
    return Result;
}

EXTERN_C int KittenZipCodecsLz5Write(
    void* Context,
    LZ5MT_Buffer* Output)
{
    KittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT ConvertedOutput;
    ConvertedOutput.Buffer = Output->buf;
    ConvertedOutput.Size = Output->size;
    ConvertedOutput.Allocated = Output->allocated;
    return ::KittenZipCodecsCommonWrite(
        reinterpret_cast<PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT>(Context),
        &ConvertedOutput);
}

EXTERN_C HRESULT WINAPI KittenZipCodecsLz5Decode(
    _In_ PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT StreamContext,
    _In_ UINT32 NumberOfThreads,
    _In_ UINT32 InputSize)
{
    LZ5MT_RdWr_t ReadWrite = {};
    ReadWrite.fn_read = ::KittenZipCodecsLz5Read;
    ReadWrite.fn_write = ::KittenZipCodecsLz5Write;
    ReadWrite.arg_read = reinterpret_cast<void*>(StreamContext);
    ReadWrite.arg_write = reinterpret_cast<void*>(StreamContext);

    LZ5MT_DCtx* Context = ::LZ5MT_createDCtx(NumberOfThreads, InputSize);
    if (!Context)
    {
        return S_FALSE;
    }

    std::size_t Result = ::LZ5MT_decompressDCtx(Context, &ReadWrite);
    if (::LZ5MT_isError(Result))
    {
        if (ERROR(canceled) == Result)
        {
            return E_ABORT;
        }

        return E_FAIL;
    }

    ::LZ5MT_freeDCtx(Context);

    return S_OK;
}
