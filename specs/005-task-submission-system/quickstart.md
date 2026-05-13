# Quickstart: Task Submission System

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- PostgreSQL running

## Backend Setup

1. **Update Database**:
   ```bash
   dotnet ef migrations add AddTaskSubmissions
   dotnet ef database update
   ```

2. **Run API**:
   ```bash
   cd backend
   dotnet run
   ```

## Frontend Setup

1. **Install Dependencies**:
   ```bash
   cd frontend
   npm install
   ```

2. **Run Development Server**:
   ```bash
   npm run dev
   ```

## Testing the Feature

1. **Log in as Admin**: Create a new task with type "Quiz" or "Text".
2. **Log in as User**: Navigate to the task and submit an answer.
   - For Quiz: Verify instant approval/rejection.
   - For Text: Verify "Pending" status.
3. **Log in as Admin**: Go to "Admin Dashboard" -> "Submissions".
   - Approve/Reject the pending submission.
   - Verify user points update.
