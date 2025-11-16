#pragma once

#include "option_base.hpp"
#include "option_id.hpp"

#include <string>

namespace gw {
namespace opts {
class ListEntries final : public gw::Option {
  public:
    ListEntries() noexcept;
    auto action(gw::Package&) noexcept -> void override;
};

class ToggleEntryClock final : public gw::Option {
  public:
    ToggleEntryClock() noexcept;

    [[nodiscard]] auto getDisplayText(const gw::Package&) noexcept
        -> std::string_view override;

    auto action(gw::Package&) noexcept -> void override final;
};

class EditEntries final : public gw::Option {
  public:
    EditEntries() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class AddNewEntry final : public gw::Option {
  public:
    AddNewEntry() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class Settings final : public gw::Option {
  public:
    Settings() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class CheckForUpdates final : public gw::Option {
  public:
    CheckForUpdates() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class ExitApp final : public gw::Option {
  public:
    ExitApp() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class ChangeEntryTitle final : public gw::Option {
  public:
    ChangeEntryTitle() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class ResetEntryClock final : public gw::Option {
  public:
    ResetEntryClock() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class DeleteEntry final : public gw::Option {
  public:
    DeleteEntry() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class GoBackToMainMenu final : public gw::Option {
  public:
    GoBackToMainMenu() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};

class ToggleClockAutosave final : public gw::Option {
  public:
    ToggleClockAutosave() noexcept;

    [[nodiscard]] auto getDisplayText(const gw::Package&) noexcept
        -> std::string_view override;

    auto action(gw::Package&) noexcept -> void override final;
};

class AdjustAutosaveInterval final : public gw::Option {
  public:
    AdjustAutosaveInterval() noexcept;
    auto action(gw::Package&) noexcept -> void override final;
};
} // namespace opts
} // namespace gw