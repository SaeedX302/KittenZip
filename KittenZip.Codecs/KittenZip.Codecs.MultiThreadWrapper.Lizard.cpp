/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.Lizard.cpp
 * PURPOSE:    Implementation for Lizard Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#include "KittenZip.Codecs.MultiThreadWrapper.Lizard.h"

#include <cstddef>

EXTERN_C int KittenZipCodecsLizardRead(
    void* Context,
    LIZARDMT_Buffer* Input)
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

EXTERN_C int KittenZipCodecsLizardWrite(
    void* Context,
    LIZARDMT_Buffer* Output)
{
    KittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT ConvertedOutput;
    ConvertedOutput.Buffer = Output->buf;
    ConvertedOutput.Size = Output->size;
    ConvertedOutput.Allocated = Output->allocated;
    return ::KittenZipCodecsCommonWrite(
        reinterpret_cast<PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT>(Context),
        &ConvertedOutput);
}

EXTERN_C HRESULT WINAPI KittenZipCodecsLizardDecode(
    _In_ PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT StreamContext,
    _In_ UINT32 NumberOfThreads,
    _In_ UINT32 InputSize)
{
    LIZARDMT_RdWr_t ReadWrite = {};
    ReadWrite.fn_read = ::KittenZipCodecsLizardRead;
    ReadWrite.fn_write = ::KittenZipCodecsLizardWrite;
    ReadWrite.arg_read = reinterpret_cast<void*>(StreamContext);
    ReadWrite.arg_write = reinterpret_cast<void*>(StreamContext);

    LIZARDMT_DCtx* Context = ::LIZARDMT_createDCtx(NumberOfThreads, InputSize);
    if (!Context)
    {
        return S_FALSE;
    }

    std::size_t Result = ::LIZARDMT_decompressDCtx(Context, &ReadWrite);
    if (::LIZARDMT_isError(Result))
    {
        if (ERROR(canceled) == Result)
        {
            return E_ABORT;
        }

        return E_FAIL;
    }

    ::LIZARDMT_freeDCtx(Context);

    return S_OK;
}
