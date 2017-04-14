# Cs2hx

CS2HX converts your C# 4.0 code into haXe code. haXe is a multi-target language, so the resulting haXe code can be converted to many other languages.

This means CS2HX can be a:
- C# to ActionScript converter
- C# to JavaScript converter
- C# to Java converter
- C# to PHP converter
- C# to C++ converter


# Project Goals

CS2HX is designed for situations where you need to share code/logic between C# and another platform, such as a Flash/Flex application or an iPhone application.

The program will parse your raw C# source files and output raw haXe source files. As C# and haXe are very similar languages in many ways, the code that comes out looks remarkably similar to the code going in. This makes debugging and profling easy.

CS2HX is great if you have an existing C# codebase you wish to port over, either as a one-time conversion or for an on-going basis for sharing logic. It is not designed to replace haXe, nor is it designed as a reason to avoid learning haXe. Rather, it eases the transition to haXe for those with existing C# codebases.

# Limitations

C# supports a lot of features that haXe does not, so there are some things that won't be supported at all such as: yield, unsafe code, operator overloads, etc.

CS2HX aims to support 90% of an average C# codebase. You should expect to have to re-factor some of your code to avoid features that CS2HX does not support.

However, even with the limitations it's still very possible to write quality C# code that can be shared across both environments. Linq, lambda, generics and OOP are supported.

Further, CS2HX will transform your code to support some C# features that haXe does not support, such as partial classes, extension methods, field initializers, and using blocks.

Please see the [limitations page](https://github.com/FizzerWL/Cs2hx/wiki/Limitations) for a detailed analysis of what is and is not supported.
