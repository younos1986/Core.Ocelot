git add .

ECHO Commit name? 
set /p done=

git commit -m  %done%

git push -u origin master




TIMEOUT /T 60
