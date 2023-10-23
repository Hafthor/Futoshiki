var f = new Futoshiki("3 ? ? ?\n" +
                      "  ^    \n" +
                      "? ?<? ?\n" +
                      "v      \n" +
                      "? ? ? ?\n" +
                      "    ^ ^\n" +
                      "?<? ? ?"); // solution: 3142 4231 2413 1324
f.Solve();
bool solved = f.Print();
if (!solved) f.PrintDebug();
return solved ? 0 : 1;