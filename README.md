# Advent of Code 2023

This repository contains my entries for the Advent of Code 2023 daily coding challenge at https://adventofcode.com/2023

All details about the goal for each day can be found on the website. The contents of this solution is just the input data and my code for generating the solutions.

Puzzle solutions are handled as unit tests. The daily puzzle is set up as a test class, and individual parts of the 
puzzle are set up as XUnit unit tests. This allows the sample test cases to be checked 
against their known solutions, and also allows for easy execution of any puzzle through the unit test explorer. Test 
puzzle output is written to the unit test output.

## Download Input Files

Advent of Code input files vary from user to user, and the author has stated [they don't want users to post their 
puzzle input publicly](https://www.reddit.com/r/adventofcode/wiki/faqs/copyright/inputs/). Thus, the .gitignore file 
is configured to keep all input files out of the repo. The base day module has been set up to auto-download the 
input file for the day. Sample input files (the small samples published in the day's puzzle page) are included in 
the repository.

To begin you'll need to login to www.adventofcode.com, open the browser dev tools,
then head to Application Tab -> Storage -> Cookies, click the cookie for the site,
then copy the value (long chain of numbers and letters) of the session cookie.

* Open the folder containing your `Core.csproj` file in terminal.
* Run the command `dotnet user-secrets init`
* Then `dotnet user-secrets set "session" "cookie"` but replace the word `cookie`
  with the actual cookie copied from the site.
* Next time the code can't fin an input file on disk, it will download from the site using your session cookie. See 
  `InputDataProvider` for details.

This only downloads input files that aren't on disk, so if you need to replace
one for whatever reason, you'll need to delete the old file first.