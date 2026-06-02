#include "pch.h"
#include "StatusBar.h"
#include "StatusBar.g.cpp"

using namespace winrt::KittenZip::Modern::implementation;

DEPENDENCY_PROPERTY_SOURCE_BOX(
    Text1,
    winrt::hstring,
    StatusBar,
    winrt::KittenZip::Modern::StatusBar
);

DEPENDENCY_PROPERTY_SOURCE_BOX(
    Text2,
    winrt::hstring,
    StatusBar,
    winrt::KittenZip::Modern::StatusBar
);

DEPENDENCY_PROPERTY_SOURCE_BOX(
    Text3,
    winrt::hstring,
    StatusBar,
    winrt::KittenZip::Modern::StatusBar
);

DEPENDENCY_PROPERTY_SOURCE_BOX(
    Text4,
    winrt::hstring,
    StatusBar,
    winrt::KittenZip::Modern::StatusBar
);
