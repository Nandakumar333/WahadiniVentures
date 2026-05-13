import React from 'react';
import ReactMarkdown from 'react-markdown';

interface CourseDescriptionProps {
  description: string;
  title?: string;
}

/**
 * Course description component with markdown rendering
 * Safely renders markdown content with styled elements
 */
export const CourseDescription: React.FC<CourseDescriptionProps> = ({
  description,
  title = 'About This Course',
}) => {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">{title}</h2>
      
      <div className="prose prose-blue dark:prose-invert max-w-none">
        <ReactMarkdown
          components={{
            // Heading styles
            h1: ({ children }) => (
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white mt-6 mb-4">
                {children}
              </h1>
            ),
            h2: ({ children }) => (
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white mt-5 mb-3">
                {children}
              </h2>
            ),
            h3: ({ children }) => (
              <h3 className="text-xl font-semibold text-gray-900 dark:text-white mt-4 mb-2">
                {children}
              </h3>
            ),
            h4: ({ children }) => (
              <h4 className="text-lg font-semibold text-gray-900 dark:text-white mt-3 mb-2">
                {children}
              </h4>
            ),
            h5: ({ children }) => (
              <h5 className="text-base font-semibold text-gray-900 dark:text-white mt-2 mb-1">
                {children}
              </h5>
            ),
            h6: ({ children }) => (
              <h6 className="text-sm font-semibold text-gray-900 dark:text-white mt-2 mb-1">
                {children}
              </h6>
            ),
            
            // Paragraph
            p: ({ children }) => (
              <p className="text-gray-700 dark:text-gray-300 leading-relaxed mb-4">{children}</p>
            ),
            
            // Lists
            ul: ({ children }) => (
              <ul className="list-disc list-inside space-y-2 mb-4 text-gray-700 dark:text-gray-300">
                {children}
              </ul>
            ),
            ol: ({ children }) => (
              <ol className="list-decimal list-inside space-y-2 mb-4 text-gray-700 dark:text-gray-300">
                {children}
              </ol>
            ),
            li: ({ children }) => <li className="ml-4">{children}</li>,
            
            // Links
            a: ({ href, children }) => (
              <a
                href={href}
                className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 underline font-medium"
                target="_blank"
                rel="noopener noreferrer"
              >
                {children}
              </a>
            ),
            
            // Code blocks
            code: ({ children, className }) => {
              const isInline = !className;
              return isInline ? (
                <code className="bg-gray-100 dark:bg-gray-700 text-red-600 dark:text-red-400 px-2 py-1 rounded text-sm font-mono">
                  {children}
                </code>
              ) : (
                <code className="block bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto text-sm font-mono mb-4">
                  {children}
                </code>
              );
            },
            pre: ({ children }) => <pre className="mb-4">{children}</pre>,
            
            // Blockquote
            blockquote: ({ children }) => (
              <blockquote className="border-l-4 border-blue-500 pl-4 italic text-gray-600 dark:text-gray-400 my-4 bg-blue-50 dark:bg-blue-900/20 py-2">
                {children}
              </blockquote>
            ),
            
            // Horizontal rule
            hr: () => <hr className="border-gray-300 dark:border-gray-700 my-6" />,
            
            // Strong/Bold
            strong: ({ children }) => (
              <strong className="font-bold text-gray-900 dark:text-white">{children}</strong>
            ),
            
            // Emphasis/Italic
            em: ({ children }) => (
              <em className="italic text-gray-800 dark:text-gray-200">{children}</em>
            ),
            
            // Table
            table: ({ children }) => (
              <div className="overflow-x-auto mb-4">
                <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 border border-gray-300 dark:border-gray-700">
                  {children}
                </table>
              </div>
            ),
            thead: ({ children }) => (
              <thead className="bg-gray-50 dark:bg-gray-700/50">{children}</thead>
            ),
            tbody: ({ children }) => (
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {children}
              </tbody>
            ),
            tr: ({ children }) => <tr>{children}</tr>,
            th: ({ children }) => (
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-700 dark:text-gray-300 uppercase tracking-wider">
                {children}
              </th>
            ),
            td: ({ children }) => (
              <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{children}</td>
            ),
          }}
        >
          {description}
        </ReactMarkdown>
      </div>
      
      {/* Empty state if no description */}
      {!description && (
        <p className="text-gray-500 dark:text-gray-400 italic">
          No description available for this course.
        </p>
      )}
    </div>
  );
};
