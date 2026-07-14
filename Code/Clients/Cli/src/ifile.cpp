#include "core/ifile.hpp"

gw::ifile::ifile(std::istream &file_reader_stream) noexcept : file_reader_stream
  {
      file_reader_stream} {
}

auto gw::ifile::read_contents() noexcept -> std::string {
  std::stringstream reader_buffer;
  reader_buffer << file_reader_stream.rdbuf();
  return reader_buffer.str();
}