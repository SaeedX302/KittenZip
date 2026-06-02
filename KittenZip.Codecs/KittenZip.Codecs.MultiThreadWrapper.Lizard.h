/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.Codecs.MultiThreadWrapper.Lizard.h
 * PURPOSE:    Definition for Lizard Multi Thread Wrapper
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#ifndef KittenZip_CODECS_MULTI_THREAD_WRAPPER_LIZARD
#define KittenZip_CODECS_MULTI_THREAD_WRAPPER_LIZARD

#include "KittenZip.Codecs.MultiThreadWrapper.Common.h"

#include <stdint.h>
#include <lizard-mt.h>

EXTERN_C int KittenZipCodecsLizardRead(
    void* Context,
    LIZARDMT_Buffer* Input);

EXTERN_C int KittenZipCodecsLizardWrite(
    void* Context,
    LIZARDMT_Buffer* Output);

EXTERN_C HRESULT WINAPI KittenZipCodecsLizardDecode(
    _In_ PKittenZip_CODECS_ZSTDMT_STREAM_CONTEXT StreamContext,
    _In_ UINT32 NumberOfThreads,
    _In_ UINT32 InputSize);

#endif // !KittenZip_CODECS_MULTI_THREAD_WRAPPER_LIZARD
