var f = new Futoshiki("3 ? ? ?\n" +
                      "  ^    \n" +
                      "? ?<? ?\n" +
                      "v      \n" +
                      "? ? ? ?\n" +
                      "    ^ ^\n" +
                      "?<? ? ?"); // solution: 3142 4231 2413 1324
f.Solve();
f.Print();
Console.WriteLine("Guesses: " + f.Guesses);
if (!f.IsSolved()) f.PrintDebug();

Random random = new Random(0); // first one is always the same
for (;;) {
    Console.WriteLine();
    f = new Futoshiki(4, random);
    f.Print();
    Console.WriteLine("Guesses: " + f.Guesses);
    Console.WriteLine("Press ENTER to show solution (or q to quit)");
    if (Console.ReadLine() == "q") break;
    f.Solve();
    f.Print();
    Console.WriteLine("Press ENTER to continue (or q to quit)");
    if (Console.ReadLine() == "q") break;
    random = new Random();
}