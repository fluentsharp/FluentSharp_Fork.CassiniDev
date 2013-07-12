Cassini fork of the branch\v6 code from http://cassinidev.codeplex.com/



Changes

This is what I did with the branch\v6 code that I downloaded from http://cassinidev.codeplex.com/ on 12/July/2013

* removed folders (with the idea of having the smallest ammount of code possible):
** .nuget
** packages
** TestContent		      (not a lot in there)
** CassiniDev.Lib.Net40       (this was just a wrapper for .Net 3.5 code)
** CassiniDev.Lib.Net35.Tests (used lots of dependencies and there were not a lot of tests in there (only 7))
** noodles.txt
** *.suo files


*inside CassiniDev.Lib.Net35 removed:
** bin
** obj

* First commmit now only had:
** CassiniDev.Lib.Net35
** CassiniDev.sln