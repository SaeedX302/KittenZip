/*
 * PROJECT:    KittenZip
 * FILE:       KittenZip.UI.h
 * PURPOSE:    Definition for KittenZip Modern UI Shared Infrastructures
 *
 * LICENSE:    The MIT License
 *
 * MAINTAINER: MouriNaruto (Kenji.Mouri@outlook.com)
 */

#ifndef KittenZip_UI
#define KittenZip_UI

#include <Mile.Helpers.CppBase.h>
#include <Mile.Helpers.CppWinRT.h>

namespace KittenZip::UI
{
    winrt::handle ShowAboutDialog(
        _In_ HWND ParentWindowHandle);

    void SpecialCommandHandler();
}

#endif // !KittenZip_UI
