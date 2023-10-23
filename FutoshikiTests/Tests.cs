namespace FutoshikiTests;

[TestClass]
public class Tests {
    [TestMethod]
    public void Test() {
        var f = new Futoshiki("3 ? ? ?\n" +
                              "  ^    \n" +
                              "? ?<? ?\n" +
                              "v      \n" +
                              "? ? ? ?\n" +
                              "    ^ ^\n" +
                              "?<? ? ?"); // solution: 3142 4231 2413 1324
        f.Solve();
        var s = f.Solution();
        Assert.AreEqual("3142\n4231\n2413\n1324", s, "Solution is incorrect");
    }
}