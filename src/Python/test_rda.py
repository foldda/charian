# Copyright (c) 2022 Foldda Pty Ltd
# Licensed under the MIT License -
# https://github.com/foldda/rda/blob/main/LICENSE

import pytest
from rda import Rda


def test_constructor_and_parsing_test():
    rda_string0 = ""
    rda0 = Rda.parse(rda_string0)
    assert "" == rda0.to_string()
    value = "Two"
    rda0.set_value(2, value)
    assert value == rda0.get_value(2)
    assert 1 == rda0.dimension()

    rda_string0 = "Xyz"
    addr = [1, 2, 3]
    rda0 = Rda.parse(rda_string0)
    assert rda_string0 == rda0.get_scalar_value()
    rda0.set_value_array(addr, value)
    assert rda_string0 == rda0.get_scalar_value()
    assert value == rda0.get_value_array(addr)
    assert 3 == rda0.dimension()

    rda_string1 = "|\\|"
    rda1 = Rda.parse(rda_string1)
    assert rda_string1 == rda1.to_string()
    assert "" == rda1.get_rda(0).to_string()
    assert "" == rda1.to_string_minimal()

    rda_string1 = "|;\\|A"
    rda1 = Rda.parse(rda_string1)
    assert 2 == rda1.dimension()
    assert rda_string1 == rda1.to_string()
    assert ";\\;A" == rda1[0].to_string()
    assert "A" == rda1[0][0].to_string()
    assert "A" == rda1[0][0][0].to_string()
    assert "A" == rda1.to_string_minimal()

    rda_string1 = "|\\|A|B"
    rda1 = Rda.parse(rda_string1)
    assert 1 == rda1.dimension()
    assert rda_string1 == rda1.to_string()

    rda_string2 = "|;,\\|"
    rda2 = Rda.parse(rda_string2)
    assert rda_string2 == rda2.to_string()
    rda_string2 = "|;.,\\|A,a.1;b|B"
    rda2 = Rda.parse(rda_string2)
    assert rda_string2 == rda2.to_string()

    rda3 = Rda()
    rda3.set_scalar_value("Michael")
    print("Dimension-0 Rda", rda3.to_string())
    assert "Michael" == rda3.to_string()
    rda3[2].set_scalar_value("Chen")
    print("Dimension-1 Rda", rda3.to_string())

    assert "Michael" == rda3[0].to_string()
    assert "Chen" == rda3[2].to_string()


def test_element_addressing_test():
    original = "|;,\\|s1|s2|s3\\s4"
    rda = Rda.parse(original)

    assert ";,\\;s1" == rda[0].to_string()
    assert "s1" == rda[0].get_scalar_value()
    assert "s3\\s4" == rda[2].get_scalar_value()

    addr1 = [1]
    assert ";,\\;s2" == rda.get_rda_array(addr1).to_string()

    rda[5].set_scalar_value("555")
    assert "555" == rda[5].to_string()
    rda[5][1].set_scalar_value("555b")
    assert "555b" == rda.get_rda_array([5, 1]).to_string()
    assert "555b" == rda.get_rda_array([5, 1, 0, 0, 0, 0]).to_string()

    assert "555" == rda[5][0].to_string()
    assert "555" == rda[5].get_scalar_value()

    rda.add_value("6666")
    assert "6666" == rda[6].to_string()


def test_input_parsing_output_formatting_test():
    rda = Rda()

    for item_value in ["SEC0", "SEC1", None, "SEC3"]:
        rda.add_value(item_value)

    r0 = rda[0]
    assert "SEC0" == r0.to_string()
    print("rda", rda.to_string_formatted())
    print("rda[0]", r0.to_string_formatted())
    print("rda[0][0]", r0[0].to_string_formatted())

    r00 = r0[0]
    s = r00.to_string()
    assert "SEC0" == s
    assert "SEC0" == rda[0][0].to_string()
    assert "SEC1" == rda[1].to_string()
    assert "" == rda.get_rda_array([2]).to_string()
    assert "SEC3" == rda.get_rda_array([3]).to_string()
    assert "" == rda.get_rda_array([4]).to_string()

    v2_formatted_string = '|;\\|\r\n "SEC0"\r\n|"SEC1"\r\n|\r\n|"SEC3"'
    print("input", v2_formatted_string)
    s1 = rda.to_string_formatted()
    assert s1 == v2_formatted_string

    v2_formatted_in = '|;\\|  \r\n\r\n SEC0| \t \r    \r\n\t S1a\r\n\t;\r  S1b\r\r\n\t;\r\n\n  S1c.1 \tS1c.2   \r\n\t;"\t S1d " \n\r\n; " S1e|\r\n|SEC3  '
    print("v2RDA-input", v2_formatted_in)

    v1_equivalent = "|;\\|SEC0|S1a;S1b;S1c.1 \tS1c.2;\t S1d ; S1e||SEC3"
    rda3 = Rda.parse(v2_formatted_in)
    assert v1_equivalent == rda3.to_string()

    v2_formatted_out = rda3.to_string_formatted()
    print("v2-formatted-output", v2_formatted_out)

    rda3_from_v2_out = Rda.parse(v2_formatted_out)
    assert rda3_from_v2_out.to_string() == rda3.to_string()


def test_value_setter_getter_escaping_test():
    rda = Rda.parse("|;,\\|sec1|sec2|,a;b,c")
    assert ";,\\;sec1" == rda.get_rda_array([0]).to_string()
    assert ";,\\;sec2" == rda.get_rda_array([1]).to_string()

    assert ",\\,sec1" == rda.get_rda_array([0, 0]).to_string()
    assert "sec1" == rda.get_rda_array([0, 0, 0]).to_string()
    assert "sec1" == rda.get_value(0)

    assert ",\\,sec2" == rda.get_rda_array([1, 0]).to_string()
    assert "sec2" == rda[1].get_scalar_value()
    assert "a" == rda.get_rda_array([2, 0, 1]).to_string()

    assert "" == rda.get_rda_array([0, 0, 1]).to_string()
    assert "" == rda.get_rda_array([5, 0]).to_string()
    assert "" == rda.get_rda_array([0, 0, 0, 6]).to_string()

    rda.set_value_array([0], "SEC1")
    rda.set_value_array([1], "SEC2")
    rda.set_value_array([2, 0, 1], "a")
    rda.set_value_array([2, 1], "b")
    rda.set_value_array([2, 1, 1], "c")

    assert "|;,\\|SEC1|SEC2|,a;b,c" == rda.to_string()
    assert "SEC2" == rda.get_value(1)
    assert "SEC1" == rda.get_value(0)
    print("rda", rda.to_string_formatted())

    rda[2].set_scalar_value("SEC3")
    assert "|;\\|SEC1|SEC2|SEC3" == rda.to_string()

    rda.set_value_array([0, 1], "SEC1b")
    assert "|;\\|SEC1;SEC1b|SEC2|SEC3" == rda.to_string()

    rda.set_value_array([0, 0, 1], "SEC1c")
    assert "|;,\\|SEC1,SEC1c;SEC1b|SEC2|SEC3" == rda.to_string()

    rda.set_value_array([0, 1, 1], "SEC1d")
    assert "|;,\\|SEC1,SEC1c;SEC1b,SEC1d|SEC2|SEC3" == rda.to_string()
    assert ";,\\;SEC1,SEC1c;SEC1b,SEC1d" == rda.get_rda_array([0]).to_string()
    assert ",\\,SEC1,SEC1c" == rda.get_rda_array([0, 0]).to_string()
    assert "SEC1d" == rda.get_rda_array([0, 1, 1]).to_string()

    rda.set_value_array([0, 1, 4], "SE;|C1d4")
    assert "SE;|C1d4" == rda.get_rda_array([0, 1, 4]).to_string()
    assert "|;,\\|SEC1,SEC1c;SEC1b,SEC1d,,,SE\\;\\|C1d4|SEC2|SEC3" == rda.to_string()

    section = rda.get_rda_array([0])
    assert ";,\\;SEC1,SEC1c;SEC1b,SEC1d,,,SE\\;|C1d4" == section.to_string()
    assert ";" == section.delimiters_in_use()[0]
    assert "," == section.delimiters_in_use()[1]
    assert "\\" == section.escape_char()

    assert "" == rda.get_rda_array([0, 1, 3]).to_string()
    assert "" == rda.get_rda_array([0, 1, 8]).to_string()

    assert rda.get_rda_array([0, 1, 0, 0]).to_string() is not None
    assert "SEC1d" == rda.get_rda_array([0, 1, 1]).to_string()

    rda.set_value_array([0, 1, 4, 2, 1], "Test Over-Index set")
    assert (
        "|;,^:\\|SEC1,SEC1c;SEC1b,SEC1d,,,SE\\;\\|C1d4^^:Test Over-Index set|SEC2|SEC3"
        == rda.to_string()
    )


def test_rda_setter_getter_escaping_test():
    rda_base = Rda.parse("|;,\\|sec0|sec1|sec2")
    print("rda-before-assigned-new-child-rda", rda_base.to_string_formatted())
    rda_sec1_new = Rda.parse("&_;/&sec1-n0&sec1-n1 /||| ; X_ x&sec1-n2_A/;B")
    print("inserted rda[1]", rda_sec1_new.to_string_formatted())
    rda_base[1] = rda_sec1_new
    print("rda-after-assigned-new-child-rda", rda_base.to_string_formatted())
    assert "sec1-n0" == rda_base[1][0].get_scalar_value()
    assert "sec1-n0" == rda_base[1].get_scalar_value()
    assert rda_sec1_new.content_equal(rda_base[1])
    assert "sec1-n1 /||| " == rda_base[1][1][0][0].to_string()
    assert " X" == rda_base[1][1][0][1].to_string()

    rda_sec3_new = Rda.parse(">;/>sec3>sec3-n1>a;b;c")
    print("inserted rda[3]", rda_sec3_new.to_string_formatted())
    rda_base.add_rda(rda_sec3_new)
    print("rda-after-adding-new-child-rda", rda_base.to_string_formatted())
    assert rda_sec3_new.content_equal(rda_base[3])


def test_escaped_value_formatting_test():
    quote_replaced = "|;,^| AAA; xx^|^;^,x | B,b; C "
    rda1 = Rda.parse(quote_replaced)
    assert quote_replaced == rda1.to_string()
    v2_formatted_rda = rda1.to_string_formatted()
    print("v2-formatted", v2_formatted_rda)
    rda2 = Rda.parse(v2_formatted_rda)
    assert quote_replaced == rda2.to_string()

    s2 = "|;,^| AAA; xx^|^;^,x | B,b; C "
    rda3 = Rda.parse(s2)
    assert s2 == rda3.to_string()
    s2f = rda3.to_string_formatted()
    print("v2-formatted", s2)
    rda4 = Rda.parse(s2f)
    assert s2 == rda4.to_string()
    
def test_rda_quick_demo_test():
    # sender creates a container
    container = Rda()
    
    # use set_value() to store some data values in container
    container.set_value(0, "One")
    container.set_value(1, "Two")
    container.set_value(2, "Three")
    
    # use to_string() to serialize the container and its content
    print(container.to_string())    # prints encoded RDA string -> "|\|One|Two|Three"
    
    # ... the string can be stored in a file, or send to another app via TCP/IP, Http, RPC, etc
    # ... and a receiver can ...
    
    # use parse() to convert an RDA string back to a container object
    received = Rda.parse("|\|One|Two|Three")
    
    # use get_value() to retrive transported value from a container 
    print(received.get_value(2))    # prints "Three" (the value at index=2 in the container)

