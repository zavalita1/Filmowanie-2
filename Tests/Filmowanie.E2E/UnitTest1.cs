using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Filmowanie.E2E;
public class UnitTest1 : PageTest
{
    [Fact]
    public async Task Test1()
    {


        await Page.GotoAsync("https://localhost:60571/");

        
        await Expect(Page).ToHaveTitleAsync("Filmowanie");
    }
}
