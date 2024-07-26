# Motivation
Improving the navigation without having to use the mouse between virtual desktops and windows for Windows 10/11

# Credits
- NickoTin: https://www.cyberforum.ru/blogs/105416/blog3671.html (for reverse engineering the virtual desktop manager)
- MScholtes: https://github.com/MScholtes/VirtualDesktop (for the C# version code)

# Usage

## EXE
Download the exe from release and run it normally, should work with/without escalated priviledges.
Because the program runs in the background, you can add it to program startups for better experience.
If the program crashes by any chance, just re-run again, if it is intermittent feel free to raise an issue.

## Build from source
The project is using .NET 8, could work with some previous/latest versions but have not done the full validation yet.
Basically with .NET SDK installed, go to the dotnet root folder, and run `dotnet build . -c Release`
There is also an incomplete version in C++, but I am having issues with building it on a company laptop due to the anti-virus and some cybersecurity policies, so probably will never finish it
