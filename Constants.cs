namespace SmartTasksAPI
{
    public static class Constants
    {
        // Entity constants
        public const int MaxChecklistNameLength = 50;

        public const int MaxTaskItemNameLength = 200;
        public const int MaxTaskItemDescLength = 5000;

        // Authentication constants
        public const int MinPasswordLength = 5;
        public const double JwtTokenValidTime = 120; // In minutes

        // Controller constants
        public const int MaxPageSize = 30;
        public const int DefaultPageSize = 10;

        // Swagger documentation 
        public const string AppTitle = "Smarttasks Web API";

        public const string AppDescription = "A Web API for managing ToDo lists. <a href=\"https://github.com/TonyC20/SmartTasksAPI\">(Source code)</a><br/><br/>" +
                                             "To use this API:<br/><br/>" +
                                             "1. Create an account at `api/v1/account/create`<br/>" +
                                             "2. Obtain a security token at `api/v1/account/authenticate` <br/>" +
                                             "3. Pass in the header `Authorization: Bearer <token>` for all subsequent requests <br/>" +
                                             "3B. Alternatively use this interactive documentation by pasting the token into " +
                                             "the green \"Authorize\" button in the top right <br/><br/>" +
                                             "Please be aware that, as this is a demo, I will reset the database if it becomes too full.";
    }
}
