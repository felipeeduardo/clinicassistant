#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$root"

ruby <<'RUBY'
require "pathname"

files = ["README.md"] + Dir.glob("docs/**/*.md").sort
broken = []

files.each do |file|
  File.read(file).scan(/!?\[[^\]]*\]\(([^)]+)\)/).flatten.each do |href|
    next if href.start_with?("http://", "https://", "mailto:", "#", "{{") || href.include?("{")

    path = href.split("#", 2).first
    next if path.empty?

    target = Pathname.new(file).dirname.join(path).cleanpath
    broken << "#{file}: #{href}" unless target.exist?
  end
end

abort("Links Markdown quebrados:\n#{broken.join("\n")}") unless broken.empty?
puts "Links Markdown internos: OK (#{files.length} arquivos)."
RUBY
