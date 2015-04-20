# Comic Book Browser
The story behind this project is that I have a bunch of .cbr files in a directory. Sometimes it feels like a pain to browse them all, so I'm developing a small little application that will display them all nicely. It'll take the cover of each file (the first image) and display them in a grid like view.

#HOW TO USE
You can drop the .exe and .dll file wherever you want; it's not too important. However, if you put the .exe+.dll file in the same directory as a .cbxml file, it will automatically read it.

What is a .cbxml file you ask? It is a file stored in every directory that has comics. It catalogs them all and stores additional data (only issue # for now...). Because there is a .cbxml file per
directory, you're free to move your folders around at will, and the comic book browser will respect your new file structure without problems.

If you're starting the comic book browser for the first time, you'll have to select file->new and find a good home for your <file>.cbxml. The only criteria for its location is that it is in a folder with .cbr/.cbz files. All files are read relatively, so no absolute file paths are stored anywhere. From there the comic browser will recursively search other directories with either .cbr/.cbz files or a .cbxml file and add them to the treeview.

You will notice that your treeview reflects your file structure. You're free to move things around in your file browser and it will reflect in your treeview (make sure to restart the comic book browser!). Double click nodes on the list to 'open' that folder and browse its comic contents.

Because at the time of writing this, there is no built in comic reader, you'll still need to have a comic book reader installed. Left clicking a comic thumbnail will open it with your default comic book reader. 

If you right click one of the thumbnails, you can set the issue number. If there is no issue number, the comic book reader will use the implied issue number. That is, starting from 0 to the amount of comics you have, sorted in alphabetical order.

Another thing to note is that you can set the default program for .cbxml files to be the comic book browser. This could potentially be convenient.

Another other thing is that you can add a Background.png to the same directory as the .exe+.dll file, and it will be set as the background image.

# The basics
The most basic functionality of this thing will be the ability to display the comics in a grid-like view, allowing the user to make sense of large amounts of files. When one of these covers is clicked, the default application on the system will open the file.

# Potential Features
Depending on how fast I get tired of working on this, the following features may or may not be added

## Ordering Feature
Sometimes my .cbr reader gets mixed up with the order of comics for some odd reason. This project will allow the user to explicitly state the order in which the comics are supposed to be in. In retrospect, this feature will be useless without the next...

## Built-in .cbr Reader
It would be nice if organzing and reading could all be done in a single application.

## Tagging System
To make locating files even easier, there could be a tagging system of sorts