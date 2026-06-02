#pragma once

#include "AddressBarTemplate.g.h"

namespace winrt::KittenZip::Modern::implementation
{
    struct AddressBarTemplate : AddressBarTemplateT<AddressBarTemplate>
    {
        AddressBarTemplate()
        {

        }
    };
}

namespace winrt::KittenZip::Modern::factory_implementation
{
    struct AddressBarTemplate : AddressBarTemplateT<
        AddressBarTemplate,
        implementation::AddressBarTemplate>
    {
    };
}
