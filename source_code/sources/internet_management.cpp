#include "internet_management.hpp"

#if defined(_WIN32) || defined(_WIN64)
// Targetting windows version (used by asio) - Win10/11
#define _WIN32_WINNT 0x0A00
#endif

#include <boost/asio.hpp>
#include <boost/beast.hpp>
#include <boost/beast/ssl.hpp>
#include <boost/json.hpp>

auto gw::attemptGithubConnection() noexcept -> bool {
    try {
        boost::asio::io_context io;
        boost::asio::ip::tcp::resolver resolver{io};
        boost::asio::ip::tcp::socket socket{io};

        constexpr const char* site_url{"github.com"};
        constexpr const char* http_port{"443"};
        auto endpoint = resolver.resolve(site_url, http_port);

        return true; // connected
    } catch (const std::exception&) {
        return false; // failed to connect
    }
}

auto gw::getLatestReleaseTagFromGithub() noexcept
    -> std::expected<std::tuple<unsigned int, unsigned int, unsigned int>,
                     bool> {
    namespace beast = boost::beast;
    namespace http = beast::http;
    namespace asio = boost::asio;
    namespace ssl = asio::ssl;
    namespace json = boost::json;

    try {
        asio::io_context io;
        ssl::context ctx{
            ssl::context::tlsv12_client}; // configure tls v1.2 client context -
                                          // for http connection
        ctx.set_default_verify_paths(); // load system trusted CA certificates
                                        // to be
        // able to connect to github api

        // tcp connection + ssl encryption
        beast::ssl_stream<beast::tcp_stream> stream{io, ctx};

        constexpr const char* const api_domain{"api.github.com"};
        constexpr const char* const api_endpoint{
            "/repos/AlexDeFoc/GameWatch/tags"};

        // Resolve & Connect
        asio::ip::tcp::resolver resolver{io};
        constexpr const char* const http_port{"443"};
        auto results = resolver.resolve(api_domain, http_port);
        beast::get_lowest_layer(stream).connect(results);

        // TSL Handshake
        stream.handshake(ssl::stream_base::client);

        // HTTP request
        constexpr int http_ver{11}; // v1.1
        http::request<http::string_body> req{http::verb::get,
                                             api_endpoint,
                                             http_ver};
        req.set(http::field::host, api_domain);
        req.set(http::field::user_agent, "GameWatchApp_UpdateChecker");

        // Send request
        http::write(stream, req);

        // Read response
        beast::flat_buffer tmp_buf;
        http::response<http::string_body> response;
        http::read(stream, tmp_buf, response);

        // Parse
        const auto parsed_contents = json::parse(response.body());
        if (!parsed_contents.is_array()) {
            // contents is not an array
            return std::unexpected<bool>(false);
        }

        const auto& arr = parsed_contents.as_array();
        if (arr.empty()) {
            // no tags found
            return std::unexpected<bool>(false);
        }

        const auto& first_tag_obj = arr[0].as_object(); // latest tag

        auto iterator =
            first_tag_obj.find("name"); // find field with version string

        if (iterator == first_tag_obj.end()) {
            // response doesn't contain the field containg the version string
            return std::unexpected<bool>(false);
        }

        // Extract version components
        const auto& version_string_from_boost = iterator->value().as_string();
        const std::string version_string{version_string_from_boost};

        size_t first_dot = version_string.find('.');
        size_t second_dot = version_string.find('.', first_dot + 1);

        if (first_dot == std::string::npos || second_dot == std::string::npos) {
            // version string is in a different format
            return std::unexpected<bool>(false);
        }

        unsigned int major = std::stoul(version_string.substr(0, first_dot));
        unsigned int minor = std::stoul(
            version_string.substr(first_dot + 1, second_dot - first_dot - 1));
        unsigned int patch = std::stoul(version_string.substr(second_dot + 1));

        return std::tuple<unsigned int, unsigned int, unsigned int>{major,
                                                                    minor,
                                                                    patch};
    } catch (...) {
        // any network function failed
        return std::unexpected<bool>(false);
    }
}