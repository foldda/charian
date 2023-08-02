from __future__ import annotations
from enum import Enum
from typing import List, Optional
import unicodedata
from i_rda import IRda

from encoding import RdaEncoding

from utils import range_contains


class FORMATTING_VERSION(Enum):
    V1: int = 1
    V2: int = 2


LINE_BREAK = "\r\n"
INDENT = "  "


class Rda(IRda):
    elements: List[Rda]

    _scalar_value: Optional[str] = None
    _home: Rda

    _encoding: RdaEncoding

    def global_encoding(self):
        return self._home.global_encoding() if self._home else self._encoding

    """Constructors"""

    def __init__(
        self,
        encoding: Optional[RdaEncoding] = None,
        parent: Optional[Rda] = None,
    ) -> None:
        self.elements = []
        self._scalar_value = None

        self._encoding = encoding if encoding else RdaEncoding()
        self._home = parent

    """Interface"""

    def to_rda(self) -> Rda:
        return self

    def from_rda(self, rda: Rda):
        if rda.dimension() == 0:
            self.set_scalar_value(rda.get_scalar_value())
        else:
            self.elements.clear()
            for child in rda.elements:
                self.elements.append(child)

    """NON HELPER METHODS"""

    @staticmethod
    def parse(rda_string: str) -> Rda:
        encoding = Rda.get_header_section_encoder(rda_string)
        rda = Rda(encoding)

        if len(encoding.delimiters) == 0:
            rda._scalar_value = rda_string
        else:
            payload = rda_string[len(encoding.delimiters) + 2 :]  # TODO: check this
            rda.parse_payload(
                payload,
                Rda.determine_parsing_format_version(payload) == FORMATTING_VERSION.V2,
            )

        return rda

    def payload(self) -> str:
        return self.get_payload(self.delimiters_in_use(), FORMATTING_VERSION.V1)

    def payload_v2(self) -> str:
        return self.get_payload(self.delimiters_in_use(), FORMATTING_VERSION.V2)

    def dimension(self) -> int:
        max_child_dimension = -1

        for c in self.elements:
            max_child_dimension = max(max_child_dimension, c.dimension())

        return max_child_dimension + 1

    def level(self) -> int:
        return self._home.level() + 1 if self._home else 0

    def get_scalar_value(self) -> str:
        if len(self.elements) > 0:
            value = self.elements[0]._scalar_value
        else:
            value = self._scalar_value

        return value if value else ""

    def set_scalar_value(self, value: str) -> None:
        self.elements: List[Rda] = []
        self._scalar_value = value

    def to_string(self) -> str:
        if self.dimension() == 0:
            return self.get_scalar_value()
        else:
            return f"{''.join(self.delimiters_in_use())}{self.escape_char()}{self.delimiters_in_use()[0]}{self.payload()}"

    def to_string_formatted(self) -> str:
        if self.dimension() == 0:
            return self.get_scalar_value()
        else:
            return f"{''.join(self.delimiters_in_use())}{self.escape_char()}{self.delimiters_in_use()[0]}{LINE_BREAK} {self.payload_v2()}"

    def __getitem__(self, index: int) -> Rda:
        return self.get_rda(index)

    def __setitem__(self, index: int, child_rda: Rda) -> None:
        self.set_rda(index, child_rda)

    def set_rda(self, index: int, child_rda: Rda) -> None:
        self.ensure_array_length(index)

        if child_rda:
            self.global_encoding().extend_delimiters(
                self.level() + child_rda.dimension() + 1
            )
            child_rda._home = self

            self.elements[index] = child_rda
        else:
            self.global_encoding().extend_delimiters(self.level() + 1)
            self.elements[index] = Rda(parent=self)

    def get_rda(self, index: int) -> Rda:
        self.global_encoding().extend_delimiters(self.level() + 1)

        if self.dimension() == 0:
            self.elements.append(Rda(parent=self))
            self.elements[0].set_scalar_value(self._scalar_value)

        self.ensure_array_length(index)

        return self.elements[index]

    def set_value(self, index: int, value: str) -> None:
        rda = Rda()
        rda.set_scalar_value(value)

        self.set_rda(index=index, child_rda=rda)

    def get_value(self, index: int) -> str:
        rda = self.get_rda(index)
        if rda:
            return rda.get_scalar_value()
        else:
            return ""

    def set_value_array(
        self, address_index_array: List[int], new_scalar_value: str
    ) -> None:
        child_rda = self.get_rda_array(address_index_array)
        if child_rda:
            child_rda.set_scalar_value(new_scalar_value)

    def get_value_array(self, address_index_array: List[int]) -> str:
        child_rda = self.get_rda_array(address_index_array)
        if child_rda:
            return child_rda.get_scalar_value()
        return ""

    def get_rda_array(self, section_int_address: Optional[List[int]]) -> Rda:
        if not section_int_address or len(section_int_address) == 0:
            return self
        else:
            child = self.get_rda(section_int_address[0])

            if len(section_int_address) == 1 or not child:
                return child
            else:
                next_level_section_index_address = section_int_address.copy()
                next_level_section_index_address.pop(0)
                return child.get_rda_array(next_level_section_index_address)

    def set_rda_array(
        self, address_index_array: List[int], new_scalar_value: str
    ) -> None:
        child_rda = self.get_rda_array(address_index_array)
        if child_rda:
            child_rda._scalar_value = new_scalar_value

    def get_value_array(self, address_index_array: List[int]) -> str:
        child_rda = self.get_rda_array(address_index_array)
        if child_rda and child_rda._scalar_value:
            return child_rda._scalar_value
        else:
            return ""

    def add_value(self, value_string: str) -> None:
        self.set_value(len(self.elements), value_string)

    def add_rda(self, rda: Rda) -> None:
        self.set_rda(len(self.elements), rda)

    def get_children_value_array(self) -> List[str]:
        result: List[str] = []

        if len(self.elements) == 0:
            result.append(self._scalar_value)
        else:
            for child in self.elements:
                result.add(child.get_scalar_value())

        return result

    def set_children_value_array(self, value: List[str]) -> None:
        self.elements: List[Rda] = []

        if not value or len(value) == 0:
            self._scalar_value = None
        else:
            for s in value:
                child = Rda(parent=self)
                child._scalar_value = s
                self.elements.append(child)

    def content_equal(self, other: Rda) -> bool:
        if self.dimension() != other.dimension() or self.length() != other.length():
            return False
        elif self.dimension() == 0:
            return self.get_scalar_value() == other.get_scalar_value()
        else:
            for i in range(self.length()):
                if self.elements[i].content_equal(other.elements[i]) == False:
                    return False
            return True

    def to_string_minimal(self):
        self.trim_solo_branch()
        return self.to_string()

    """HELPER METHODS"""

    def child_delimiter(self) -> str:
        return self.global_encoding().delimiters[self.level()]

    def escape_char(self) -> str:
        return self.global_encoding().escape_char

    def length(self) -> int:
        return len(self.elements)

    def delimiters_in_use(self) -> List[str]:
        level = self.level()
        dimension = self.dimension()
        subarray: List[str] = (
            self.global_encoding().delimiters[level : level + dimension].copy()
        )
        return subarray

    def parse_payload(self, payload_string: str, v2_formatted: bool) -> None:
        self.elements: List[Rda] = []

        self._scalar_value = Rda.unescape(
            payload_string,
            self.global_encoding().delimiters,
            self.escape_char(),
            v2_formatted,
        )

        if self.level() < len(self.global_encoding().delimiters):
            sections: List[str] = self.parse_children_content_sections(payload_string)

            for child_payload in sections:
                child = Rda(parent=self)
                child.parse_payload(child_payload, v2_formatted)

                self.elements.append(child)

    def trim_solo_branch(self) -> bool:
        if len(self.elements) == 1:
            if (
                self.elements[0].dimension() == 0
                or self.elements[0].trim_solo_branch() == True
            ):
                self.set_scalar_value(self.elements[0].get_scalar_value())
                return True
            else:
                return False
        else:
            for child in self.elements:
                child.trim_solo_branch()
            return False

    def __str__(self) -> str:
        return self.to_string()

    def __getitem__(self, index: int) -> Rda:
        return self.get_rda(index)

    def __setitem__(self, index: int, child_rda: Rda) -> None:
        self.set_rda(index, child_rda)

    @staticmethod
    def determine_parsing_format_version(payload_string: str) -> FORMATTING_VERSION:
        value_char_array = list(payload_string)

        for i in range(len(value_char_array)):
            curr_char = value_char_array[i]

            if not str.isspace(curr_char):
                return FORMATTING_VERSION.V1
            elif curr_char == "\n":
                return FORMATTING_VERSION.V2

        return FORMATTING_VERSION.V1

    @staticmethod
    def get_header_section_encoder(rda_string: str) -> RdaEncoding:
        if (not rda_string or len(rda_string) == 0) == False:
            value_char_array: List[str] = list(rda_string)

            for i in range(len(value_char_array)):
                curr_char = value_char_array[i]

                if (
                    str.isspace(curr_char)
                    or unicodedata.category(curr_char) == "C"
                    or RdaEncoding.DOUBLE_QUOTE == curr_char
                ):
                    break

                if range_contains(value_char_array, 0, i, curr_char):
                    if curr_char == value_char_array[0] and i > 1:
                        header_section_end_index = i
                        delimiters = value_char_array[: header_section_end_index - 1]

                        return RdaEncoding(
                            delimiters, value_char_array[header_section_end_index - 1]
                        )
                    else:
                        break

        return RdaEncoding()

    @staticmethod
    def unescape(
        payload_string: str, delimiters: List[str], escape_char: str, v2_formatted: bool
    ) -> str:
        if not payload_string or len(payload_string) == 0:
            return payload_string
        elif len(payload_string.strip()) < 2:
            return payload_string if not v2_formatted else payload_string.strip()

        value_chars: List[str]

        if v2_formatted:
            payload_string = payload_string.strip()
            payload_string = payload_string.strip(RdaEncoding.DOUBLE_QUOTE)

            value_chars = list(payload_string)

            # NOTE: changed logic here
        else:
            value_chars = list(payload_string)

        unescaped = ""
        escaping = False

        for i in range(len(value_chars) - 1):
            current_char = value_chars[i]
            if current_char == escape_char:
                escaping = not escaping
            else:
                escaping = False

            next_char = value_chars[i + 1]
            if escaping and (
                next_char == escape_char
                or range_contains(delimiters, 0, len(delimiters), next_char)
            ):
                continue
            unescaped += current_char
        unescaped += value_chars[len(value_chars) - 1]

        return unescaped

    def indent(self) -> str:
        if not self._home or len(self._home.elements) == 1:
            return ""
        else:
            return self._home.indent() + INDENT

    def get_payload(
        self, delimiter_chars: List[str], formatting_version: FORMATTING_VERSION
    ):

        apply_formatting = formatting_version == FORMATTING_VERSION.V2

        result = ""

        if self.last_non_dummy_index() < 0:
            escaped = Rda.escape(
                self._scalar_value,
                delimiter_chars,
                self.escape_char(),
                apply_formatting,
            )
            if escaped:
                result += escaped

        else:
            for i in range(self.last_non_dummy_index() + 1):
                child = self.elements[i]
                if apply_formatting:
                    result += self.get_formatting_prefix(i)

                result += "" if i == 0 else self.child_delimiter()
                result += child.get_payload(delimiter_chars, formatting_version)

        return result

    def get_formatting_prefix(self, index: int) -> str:
        if index == 0:
            return INDENT if len(self.elements) > 1 and self._home != None else ""
        else:
            return LINE_BREAK + self.indent()

    def is_dummy(self):
        if len(self.elements) == 0:
            return self._scalar_value is None
        else:
            for child in self.elements:
                if child.is_dummy() == False:
                    return False
            return True

    def last_non_dummy_index(self) -> int:
        last_index = len(self.elements) - 1
        while last_index >= 0 and self.elements[last_index].is_dummy() == True:
            last_index -= 1

        return last_index

    def ensure_array_length(self, index: int) -> None:
        diff = index - len(self.elements) + 1

        while diff > 0:
            dummy = Rda(parent=self)
            self.elements.append(dummy)
            diff -= 1

    def parse_children_content_sections(self, parent_payload: str) -> List[str]:
        result: List[str] = []

        if not parent_payload or len(parent_payload) == 0:
            result.append(parent_payload)
            return result

        escaping = False
        child_section_start_index = 0
        value_char_array: List[str] = list(parent_payload)

        for curr_char_index in range(child_section_start_index, len(value_char_array)):
            curr_char = value_char_array[curr_char_index]
            if curr_char == self.escape_char():
                escaping = not escaping
                continue
            elif not escaping and curr_char == self.child_delimiter():
                section_length = curr_char_index - child_section_start_index
                child_payload = "".join(
                    value_char_array[
                        child_section_start_index : child_section_start_index
                        + section_length
                    ]
                )
                result.append(child_payload)

                child_section_start_index = curr_char_index + 1
            escaping = False

        if child_section_start_index < len(value_char_array):
            length = len(value_char_array) - child_section_start_index
            last_section_value = "".join(
                value_char_array[
                    child_section_start_index : child_section_start_index + length
                ]
            )
            result.append(last_section_value)
        return result

    @staticmethod
    def escape(
        element_value: str,
        delimiters_in_use: List[str],
        escape_char: str,
        apply_formatting: bool,
    ) -> str:
        if not element_value:
            return element_value

        escaped = ""
        for c in list(element_value):
            if escape_char == c:
                escaped += escape_char
            else:
                for delimiter in delimiters_in_use:
                    if delimiter == c:
                        escaped += escape_char
                        break

            escaped += c

        if apply_formatting:
            escaped = RdaEncoding.DOUBLE_QUOTE + escaped
            escaped += RdaEncoding.DOUBLE_QUOTE

        return escaped

    def __repr__(self) -> str:
        return self.to_string()
