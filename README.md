# Multi-Template Generator
##### Multi-template generator for Visual Studio 2019

Visual Studio add-in for generating solution templates (Project Collections) for existing solutions and existing project templates.

Limited support for solution folders. Works with projects only, not files and folders.

The source code also includes a WPF project so the Generator can also be executed as a stand-alone executable. You will have to compile this exe yourself.

#### Guide

1. Open the solution/vstemplate you want to create a multi-project template from.
2. Enter the required information for the solution template.
3. Select the projects you want to use using check boxes.
4. Select a output directory.
5. Click Generate to generate selected project templates.

![Screenshot](https://app.quickinstaller.net/downloads/multi-template-generator/Multi-Template_Generator_Window.png "Screenshot")

#### Generation Process
1. Generate selected project templates and copy template icon and preview image to %OUTPUTDIR%\%TEMPLATE-NAME% directory.
2. Generate solution template and copy template icon and preview image to output directory.
3. Copy all files and sub-folders for each project to %OUTPUTDIR%\%TEMPLATE-NAME% directory.
4. Create zip file: %OUTPUTDIR%\%SOLUTION-TEMPLATE-NAME%.zip
5. Optionally copies the zip file to Visual Studio Project Temlpates directory.

##### Microsoft's documentation can be found here:
Reference: https://docs.microsoft.com/en-us/visualstudio/extensibility/visual-studio-template-schema-reference

Tempalte Data: https://docs.microsoft.com/en-us/visualstudio/extensibility/templatedata-element-visual-studio-templates

Project template: https://docs.microsoft.com/en-us/visualstudio/extensibility/project-element-visual-studio-templates

Tags: https://docs.microsoft.com/en-us/visualstudio/ide/template-tags?view=vs-2019

