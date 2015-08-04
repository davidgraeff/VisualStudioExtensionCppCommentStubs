# Visual Studio Extension
Generate Visual Studio XML, native Doxygen, Java-Style or Qt-Style documentation comment stubs for c++ when three forward slashes are typed.
Inspired by CppTripleSlash by chakrab (https://cpptripleslash.codeplex.com/).

## Usage
Just enter three slashes in front of a c++ class or function to generate a Doxygen parseable comment stub.
If you enter three slashes after a variable in the same line, then automatically ///< will be inserted so that the comment 
reference the variable before the comment.

### Change comment style
Go to settings -> C++ -> Comment Style to choose between given comment stub styles.

# Build
To build the extension, you probably need a Key.snk file (Visual Studio Strong Name Key File). Please refer
to the MSDN pages for generating one.