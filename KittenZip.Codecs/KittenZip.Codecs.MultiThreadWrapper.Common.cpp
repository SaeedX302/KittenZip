/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.Common.cpp
 * PURPOSE:    Implementation for Common Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#include "KittenZip.Codecs.MultiThreadWrapper.Common.h"

#include "KittenZip.Codecs.SevenZipWrapper.h"

EXTERN_C int KittenZipCodecsCommonRead(
    PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT Context,
    PKittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT Input)
{
    SIZE_T ProcessedSize = 0;
    HRESULT hr = ::KittenZipCodecsReadInputStream(
        Context->InputStream,
        Input->Buffer,
        Input->Size,
        &ProcessedSize);

    // catch errors
    if (E_ABORT == hr)
    {
        return -2;
    }
    else if (E_OUTOFMEMORY == hr)
    {
        return -3;
    }

    // some other error -> read_fail
    if (S_OK != hr)
    {
        return -1;
    }

    Input->Size = ProcessedSize;
    *Context->ProcessedInputSize += ProcessedSize;

    return 0;
}

EXTERN_C int KittenZipCodecsCommonWrite(
    PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT Context,
    PKittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT Output)
{
    UINT32 Todo = static_cast<UINT32>(Output->Size);
    UINT32 Done = 0;

    while (Todo)
    {
        UINT32 Block = 0;

        HRESULT hr = Context->OutputStream->Write(
            reinterpret_cast<PBYTE>(Output->Buffer) + Done,
            Todo,
            &Block);

        // catch errors
        if (E_ABORT == hr)
        {
            return -2;
        }
        else if (E_OUTOFMEMORY == hr)
        {
            return -3;
        }

        Done += Block;

        if (SEVENZIP_ERROR_WRITING_WAS_CUT == hr)
        {
            break;
        }

        // some other error -> write_fail
        if (S_OK != hr)
        {
            return -1;
        }

        if (!Block)
        {
            return -1;
        }

        Todo -= Block;
    }

    *Context->ProcessedOutputSize += Done;

    // we need no lock here, cause only one thread can write
    if (Context->Progress)
    {
        Context->Progress->SetRatioInfo(
            Context->ProcessedInputSize,
            Context->ProcessedOutputSize);
    }

    return 0;
}
