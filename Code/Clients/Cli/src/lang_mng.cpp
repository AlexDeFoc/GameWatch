#include "core/lang_mng.hpp"

gw::lang_mng::lang_mng() noexcept {
  static auto instance_already_exists{false};

  if (!instance_already_exists) {
    instance_already_exists = true;
  } else {
    assert(false && "Cannot create multiple instances");
  }
}