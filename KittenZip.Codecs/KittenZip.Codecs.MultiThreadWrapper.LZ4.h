/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.LZ4.h
 * PURPOSE:    Definition for LZ4 Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#ifndef KittenZip_CODECS_MULTI_THREAD_WRAPPER_LZ4
#define KittenZip_CODECS_MULTI_THREAD_WRAPPER_LZ4

#include "KittenZip.Codecs.MultiThreadWrapper.Common.h"

#include <stdint.h>
#include <lz4-mt.h>

EXTERN_C int KittenZipCodecsLz4Read(
    void* Context,
    LZ4MT_Buffer* Input);

EXTERN_C int KittenZipCodecsLz4Write(
    void* Context,
    LZ4MT_Buffer* Output);

EXTERN_C HRESULT WINAPI KittenZipCodecsLz4Decode(
    _In_ PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT StreamContext,
    _In_ UINT32 NumberOfThreads,
    _In_ UINT32 InputSize);

#endif // !KittenZip_CODECS_MULTI_THREAD_WRAPPER_LZ4
