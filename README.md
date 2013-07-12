Cassinidev Fork
===============

Cassini fork of the branch\v6 code from http://cassinidev.codeplex.com/



Changes:
=======

- added project UnitTests_FluentSharp_Fork.CassiniDev

- renamed project to FluentSharp_Fork.CassiniDev


- This is what I did with the branch\v6 code that I downloaded from http://cassinidev.codeplex.com/ on 12/July/2013

* removed folders (with the idea of having the smallest ammount of code possible):
* .nuget
** packages
** TestContent		      (not a lot in there)
** CassiniDev.Lib.Net40       (this was just a wrapper for .Net 3.5 code)
** CassiniDev.Lib.Net35.Tests (used lots of dependencies and there were not a lot of tests in there (only 7))
** noodles.txt
** *.suo files


*inside CassiniDev.Lib.Net35 removed:
** bin
** obj
** Program.cs (not used in VS project)
** Views (not used in VS CassiniDev.Lib.Net35 project (with the idea to have as little dependencies as possible)

* First commmit now only had:
** CassiniDev.Lib.Net35
** CassiniDev.sln

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
** Program.cs (not used in VS project)
** Views (not used in VS CassiniDev.Lib.Net35 project (with the idea to have as little dependencies as possible)

* First commmit now only had:
** CassiniDev.Lib.Net35
** CassiniDev.sln