# MSBuildLogAnalyzer

![logo](Media/MSBuild.png)

I needed a way to analyze and understand the complicated process of visual studio builds, so I created this tool to visualize the things that happen under the hood. It helped me to identify the most time consuming parts in big solutions.

The tool can display MSBuild logs created with 'diagnostic' level. You can start a multi-threaded build that creates the necessary logs using the 'Run build' button. After that use the 'Open log file' to open it. The tool provides 4 views to analyze the build logs:

## Timeline
It shows you the target and project calls in the selected project. You can step into the project calls to see more details.
![Timeline tab](Media/Timeline.png?raw=true)
![Timeline tab stepped into project call](Media/TimelineStepInto.png?raw=true)

## Project summary
It calculates the time it took to build the different projects and shows a comparison. It ignores the parts where project builds are waiting for dependencies.
![Project summary tab](Media/ProjectSummary.png?raw=true)

## Project timeline
It flattens the hierarchy of project and target calls and shows the projects in a timeline to see when do they work and wait for dependencies.
![Project timeline tab](Media/ProjectTimeline.png?raw=true)

## Target summary
It collects all the called targets, counts them, calculates the total duration for them and shows a comparison. The targets about waiting for dependencies are grayed out.
![Target summary tab](Media/TargetSummary.png?raw=true)
