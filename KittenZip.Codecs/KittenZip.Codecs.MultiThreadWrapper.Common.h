/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.Common.h
 * PURPOSE:    Definition for Common Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#ifndef KittenZip_CODECS_MULTI_THREAD_WRAPPER_COMMON
#define KittenZip_CODECS_MULTI_THREAD_WRAPPER_COMMON

#if defined(ZIP7_INC_COMPILER_H) || defined(__7Z_COMPILER_H)
#include <Windows.h>
#else
#include <KittenZip.Specification.SevenZip.h>
#endif

typedef struct _KittenZip_CODECS_ZSTDMT_STREAM_CONTEXT
{
    ISequentialInStream* InputStream;
    ISequentialOutStream* OutputStream;
    ICompressProgressInfo* Progress;
    PUINT64 ProcessedInputSize;
    PUINT64 ProcessedOutputSize;
} KittenZip_CODECS_ZSTDMT_STREAM_CONTEXT, *PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT;

typedef struct _KittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT
{
    PVOID Buffer;
    SIZE_T Size;
    SIZE_T Allocated;
} KittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT, *PKittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT;

EXTERN_C int KittenZipCodecsCommonRead(
    PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT Context,
    PKittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT Input);

EXTERN_C int KittenZipCodecsCommonWrite(
    PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT Context,
    PKittenZip_CODECS_ZSTDMT_BUFFER_CONTEXT Output);

#endif // !KittenZip_CODECS_MULTI_THREAD_WRAPPER_COMMON
