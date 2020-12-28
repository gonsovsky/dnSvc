
namespace SmbLibraryStd.RPC
{
    public enum RejectionReason : ushort
    {
        NotSpecified,
        AbstractSyntaxNotSupported,
        ProposedTransferSyntaxesNotSupported,
        LocalLimitExceeded,
    }
}
