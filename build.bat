@echo off
tools\FAKE\Fake.exe build.fsx "%1"
exit /b %errorlevel%