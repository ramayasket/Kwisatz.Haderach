namespace Kw.Common.Test
{
    class Program
    {
        internal static void Main()
        {
            var test = new JDynamicIssues();

            test.Construction();
            test.Alteration();
            test.ListMembers();
            //test.QueryMembers();
        }
    }
}
