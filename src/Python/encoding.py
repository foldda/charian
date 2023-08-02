# Copyright (c) 2022 Foldda Pty Ltd
# Licensed under the MIT License -
# https://github.com/foldda/rda/blob/main/LICENSE

from typing import List, Optional

from utils import range_contains


class RdaEncoding:
    DOUBLE_QUOTE = '"'
    DEFAULT_DELIMITER_CHARS = [
        "|",
        ";",
        ",",
        "^",
        ":",
        "~",
        "$",
        "&",
        "#",
        "=",
        "*",
        ".",
        "'",
        "@",
        "_",
        "%",
        "/",
        "!",
        "?",
        ">",
        "<",
        "+",
        "-",
        "{",
        "}",
        "[",
        "]",
        "(",
        ")",
        "`",
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
    ]

    DEFAULT_ESCAPE_CHAR = "\\"

    def __init__(
        self,
        custom_delimiters: List[str] = [],
        escape_char: Optional[str] = None,
    ) -> None:
        self.delimiters: List[str] = custom_delimiters 
        self.escape_char = escape_char if escape_char else RdaEncoding.DEFAULT_ESCAPE_CHAR

    def extend_delimiters(self, new_level: int):
        if new_level <= len(self.delimiters):
            return
        elif new_level < len(RdaEncoding.DEFAULT_DELIMITER_CHARS):
            new_level_delimiters: List[str] = self.delimiters.copy()

            # Add empty values up til level length
            while len(new_level_delimiters) < new_level:
                new_level_delimiters.append('')

            existing_range_index = len(self.delimiters)

            for candidate_delimiter_char in RdaEncoding.DEFAULT_DELIMITER_CHARS:
                if not range_contains(new_level_delimiters, 0, existing_range_index, candidate_delimiter_char):
                    new_level_delimiters[existing_range_index] = candidate_delimiter_char
                    existing_range_index += 1

                    if existing_range_index == new_level:
                        self.delimiters = new_level_delimiters
                        return
        
        raise Exception(f"Maximum RDA dimension-limit ({new_level}) reached, no child RDA can be accepted.")