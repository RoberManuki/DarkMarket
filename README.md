# DarkMarket Project

## Overview
DarkMarket is a Blazor application designed with a dark/gothic aesthetic, featuring a responsive layout that includes a sidebar menu, header, and footer. The application is structured to facilitate user authentication and provides a pre-styled body for development.

## Features
- **Responsive Design**: The layout adapts to different screen sizes, ensuring a seamless user experience across devices.
- **Dark/Gothic Styling**: The application incorporates a dark theme to enhance visual appeal and user engagement.
- **Sidebar Menu**: A navigation menu that allows users to easily access different sections of the application.
- **Header with Social Links**: Includes buttons for dark mode toggle, Instagram, and GitHub.
- **Login/Authentication Page**: A dedicated page for user login and authentication.
- **Footer**: Displays basic information, including copyright details.

## Project Structure
- **DarkMarket.sln**: Solution file containing project configuration.
- **Program.cs**: Entry point for the Blazor application.
- **App.razor**: Root component that sets up routing.
- **wwwroot/css/site.css**: Custom CSS styles for dark/gothic design.
- **Shared Components**: Includes MainLayout, Sidebar, Header, Footer, and NavMenu.
- **Pages**: Contains Index and Login pages.
- **Data and Services**: UserService and AuthService for handling user operations and authentication logic.
- **Models**: User model definition.

## Getting Started
To run the application, ensure you have the .NET SDK installed. Clone the repository and navigate to the project directory. Use the following command to run the application:

```
dotnet run
```

## License
This project is licensed under the MIT License. See the LICENSE file for more details.

## Acknowledgments
Special thanks to the Blazor community and contributors for their support and resources.