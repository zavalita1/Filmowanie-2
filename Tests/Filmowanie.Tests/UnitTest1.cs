using System.Linq.Expressions;

namespace Filmowanie.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Expression<Func<Test, object>> act = x => x.XYZ;
        var act2 = act.Body as MemberExpression;
        var act3 = act2.Expression as ParameterExpression;

        var b = 10;
    }

    public class Test
    {
        public string XYZ { get; set; }
    }
}