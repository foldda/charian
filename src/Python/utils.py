from typing import List


def range_contains(
    source_char_array: List[str],
    range_start_index: int,
    range_end_index: int,
    target_char: str,
) -> bool:
    for i in range(range_start_index, range_end_index):
        if source_char_array[i] == target_char:
            return True
    return False