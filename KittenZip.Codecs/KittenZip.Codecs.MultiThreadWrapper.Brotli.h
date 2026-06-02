/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.Brotli.h
 * PURPOSE:    Definition for Brotli Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#ifndef KittenZip_CODECS_MULTI_THREAD_WRAPPER_BROTLI
#define KittenZip_CODECS_MULTI_THREAD_WRAPPER_BROTLI

#include "KittenZip.Codecs.MultiThreadWrapper.Common.h"

#include <stdint.h>
#include <brotli-mt.h>

EXTERN_C int KittenZipCodecsBrotliRead(
    void* Context,
    BROTLIMT_Buffer* Input);

EXTERN_C int KittenZipCodecsBrotliWrite(
    void* Context,
    BROTLIMT_Buffer* Output);

EXTERN_C HRESULT WINAPI KittenZipCodecsBrotliDecode(
    _In_ PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT StreamContext,
    _In_ UINT32 NumberOfThreads,
    _In_ UINT32 InputSize);

#endif // !KittenZip_CODECS_MULTI_THREAD_WRAPPER_BROTLI
