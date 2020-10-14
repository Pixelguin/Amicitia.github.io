# Amicitia.github.io
This is a static set of HTML pages created dynamically using a C# program that also lives in this repository.
The program iterates through entries in a database to fill these pages with "posts" (mods/tools/cheats/guides with user-provided metadata).

# Why Static?
Github Pages provides a free and collaborative hosting solution, but only serves static pages.
While a self-hosted website could allow dynamic page building, the functionality can be replicated to work on Github using the included program.
Project Collaborators can submit their own changes to the page building code or database without using anything but GitHub and their editing tools of choice.
Now the community can work together to host material without relying on a single webmaster.

# How to Contribute to the Database?
- Clone the repository using Git or Github Desktop.
- Open the solution in Visual Studio (or your preferred IDE).
- Open the TSV files in /db/ (using Google Sheets, Excel, Notepad++ etc.)
- Add new rows or edit existing ones as needed.
- Run the program to generate new pages.
- Commit changes to your own fork and open a pull request.
