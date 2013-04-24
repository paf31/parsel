parsel
======

A just-in-time compiler for recursive descent parsers.

Parsers are represented as data structures which involve expression trees. Every parser provides a compile method which can be used to generate an efficient recursive descent parser implemented as a dynamic method.

The API is similar to working with the monoidal functor instance of the parser monad. This means that concatenation is allowed, but using the result of a successful parse to determine the next parser is not.

License
=======

See LICENSE.txt
