/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.Brotli.cpp
 * PURPOSE:    Implementation for Brotli Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#include "KittenZip.Codecs.MultiThreadWrapper.Brotli.h"

#include <cstddef>

EXTERN_C int KittenZipCodecsBrotliRead(
    void* Context,
    BROTLIMT_Buffer* Input)
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

EXTERN_C int KittenZipCodecsBrotliWrite(
    void* Context,
    BROTLIMT_Buffer* Output)
{
    KittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT ConvertedOutput;
    ConvertedOutput.Buffer = Output->buf;
    ConvertedOutput.Size = Output->size;
    ConvertedOutput.Allocated = Output->allocated;
    return ::KittenZipCodecsCommonWrite(
        reinterpret_cast<PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT>(Context),
        &ConvertedOutput);
}

EXTERN_C HRESULT WINAPI KittenZipCodecsBrotliDecode(
    _In_ PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT StreamContext,
    _In_ UINT32 NumberOfThreads,
    _In_ UINT32 InputSize)
{
    BROTLIMT_RdWr_t ReadWrite = {};
    ReadWrite.fn_read = ::KittenZipCodecsBrotliRead;
    ReadWrite.fn_write = ::KittenZipCodecsBrotliWrite;
    ReadWrite.arg_read = reinterpret_cast<void*>(StreamContext);
    ReadWrite.arg_write = reinterpret_cast<void*>(StreamContext);

    BROTLIMT_DCtx* Context = ::BROTLIMT_createDCtx(NumberOfThreads, InputSize);
    if (!Context)
    {
        return S_FALSE;
    }

    std::size_t Result = ::BROTLIMT_decompressDCtx(Context, &ReadWrite);
    if (::BROTLIMT_isError(Result))
    {
        if (MT_ERROR(canceled) == Result)
        {
            return E_ABORT;
        }

        return E_FAIL;
    }

    ::BROTLIMT_freeDCtx(Context);

    return S_OK;
}
