using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Abstraction;


//
// Summary:
//     Interface for a Protocol Buffers message, supporting basic operations required
//     for serialization.
public interface IMessage
{
    //
    // Summary:
    //     Descriptor for this message. All instances are expected to return the same descriptor,
    //     and for generated types this will be an explicitly-implemented member, returning
    //     the same value as the static property declared on the type.
    MessageDescriptor Descriptor { get; }

    //
    // Summary:
    //     Merges the data from the specified coded input stream with the current message.
    //
    // Parameters:
    //   input:
    //
    // Remarks:
    //     See the user guide for precise merge semantics.
    void MergeFrom(CodedInputStream input);

    //
    // Summary:
    //     Writes the data to the given coded output stream.
    //
    // Parameters:
    //   output:
    //     Coded output stream to write the data to. Must not be null.
    void WriteTo(CodedOutputStream output);

    //
    // Summary:
    //     Calculates the size of this message in Protocol Buffer wire format, in bytes.
    //
    // Returns:
    //     The number of bytes required to write this message to a coded output stream.
    int CalculateSize();
}

//
// Summary:
//     Generic interface for a Protocol Buffers message, where the type parameter is
//     expected to be the same type as the implementation class.
//
// Type parameters:
//   T:
//     The message type.
public interface IMessage<T> : IMessage, IEquatable<T>, IDeepCloneable<T> where T : IMessage<T>
{
    //
    // Summary:
    //     Merges the given message into this one.
    //
    // Parameters:
    //   message:
    //     The message to merge with this one. Must not be null.
    //
    // Remarks:
    //     See the user guide for precise merge semantics.
    void MergeFrom(T message);
}
