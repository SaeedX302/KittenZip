/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.LZ5.h
 * PURPOSE:    Definition for LZ5 Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#ifndef KittenZip_CODECS_MULTI_THREAD_WRAPPER_LZ5
#define KittenZip_CODECS_MULTI_THREAD_WRAPPER_LZ5

#include "KittenZip.Codecs.MultiThreadWrapper.Common.h"

#include <stdint.h>
#include <lz5-mt.h>

EXTERN_C int KittenZipCodecsLz5Read(
    void* Context,
    LZ5MT_Buffer* Input);

EXTERN_C int KittenZipCodecsLz5Write(
    void* Context,
    LZ5MT_Buffer* Output);

EXTERN_C HRESULT WINAPI KittenZipCodecsLz5Decode(
    _In_ PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT StreamContext,
    _In_ UINT32 NumberOfThreads,
    _In_ UINT32 InputSize);

#endif // !KittenZip_CODECS_MULTI_THREAD_WRAPPER_LZ5
