// Windows/Control/Edit.h

#ifndef ZIP7_INC_WINDOWS_CONTROL_EDIT_H
#define ZIP7_INC_WINDOWS_CONTROL_EDIT_H

// **************** KittenZip Modification Start ****************
#include <Shlwapi.h>
// **************** KittenZip Modification End ****************

#include "../Window.h"

namespace NWindows {
namespace NControl {

class CEdit: public CWindow
{
public:
  // **************** KittenZip Modification Start ****************
  void Attach(HWND newWindow) override {
    _window = newWindow;
    ::SHAutoComplete(
        _window,
        SHACF_AUTOAPPEND_FORCE_OFF | SHACF_AUTOSUGGEST_FORCE_OFF);
  }
  // **************** KittenZip Modification End ****************
  void SetPasswordChar(WPARAM c) { SendMsg(EM_SETPASSWORDCHAR, c); }
};

}}

#endif
