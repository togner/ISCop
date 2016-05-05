namespace ISCop
{
    public enum SourceAccessMode : int
    {
        OpenRowSet = 0,
        OpenRowSetVariable = 1,
        SqlCommand = 2,
        SqlCommandVariable = 3
    }

    public enum DestinationAccessMode : int
    {
        OpenRowSet = 0,
        OpenRowSetVariable = 1,
        SqlCommand = 2,
        OpenRowSetFastLoad = 3,
        OpenRowSetFastLoadVariable = 4
    }
}
