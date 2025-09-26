import { useState } from 'react';
import { CreateTaskForm } from './components/tasks/CreateTaskForm';
import { TaskList } from './components/tasks/TaskList';
import { ThemeToggle } from './components/ui/ThemeToggle';
import { UserSelect } from './components/ui/UserSelect';
import { SkipLinks } from './components/ui/SkipLinks';
import { ConnectionStatus } from './components/ui/ConnectionStatus';
import { UserProvider, useUser } from './contexts/UserProvider';
import './App.css';

function AppContent() {
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const { currentUserId, setCurrentUserId, isUserSelected } = useUser();

  const handleTaskCreated = () => {
    setRefreshTrigger(prev => prev + 1);
  };

  return (
    <>
      <SkipLinks />
      <ConnectionStatus />
      <div className="min-h-screen min-w-[600px] bg-background text-foreground">
        <header className="bg-card border-b border-border dark:border-gray-600 sticky top-0 z-10 w-full">
          <div className="container mx-auto px-4 py-4 flex justify-between items-center">
            <div className="flex items-center gap-2">
              <img src="/logo.svg" alt="TaskPuma Logo" className="w-12 h-12" />
              <h1 className="text-2xl font-bold">TaskPuma</h1>
            </div>
            <nav aria-label="User controls" className="flex items-center gap-4">
              <UserSelect value={currentUserId} onChange={setCurrentUserId} />
              <ThemeToggle />
            </nav>
          </div>
        </header>

        <main id="main-content" className="container mx-auto px-4 py-8">
          {isUserSelected ? (
            <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
              <section id="task-form" className="md:col-span-4" aria-labelledby="task-form-heading">
                <CreateTaskForm onTaskCreated={handleTaskCreated} />
              </section>

              <section id="task-list" className="md:col-span-8" aria-labelledby="task-list-heading">
                <TaskList key={refreshTrigger} />
              </section>
            </div>
          ) : (
            <div className="text-center py-8" role="status" aria-live="polite">
              <h2 className="text-xl font-semibold mb-2">Please Select a User</h2>
              <p className="text-muted-foreground">Select a user from the dropdown menu above to manage their tasks.</p>
            </div>
          )}
        </main>

        <footer className="bg-card border-t border-border dark:border-gray-600 mt-auto py-6" role="contentinfo">
          <div className="container mx-auto px-4 text-center text-muted-foreground text-sm">
            <p>TaskPuma • Built with React, TypeScript, and .NET</p>
          </div>
        </footer>
      </div>
    </>
  );
}

function App() {
  return (
    <UserProvider>
      <AppContent />
    </UserProvider>
  );
}

export default App;
