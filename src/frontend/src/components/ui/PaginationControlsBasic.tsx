import type { PaginationInfo } from '../../lib/types';

interface PaginationControlsBasicProps {
  pagination: PaginationInfo;
  onPageChange: (page: number) => void;
}

export function PaginationControlsBasic({ pagination, onPageChange }: PaginationControlsBasicProps) {
  return (
    <div className="flex justify-end items-center text-sm text-muted-foreground" data-testid="pagination-basic">
      <button
        onClick={() => onPageChange(pagination.pageNumber - 1)}
        disabled={!pagination.hasPreviousPage}
        data-testid="prev-page"
        className="px-2 py-1 rounded border border-border hover:bg-accent hover:text-accent-foreground transition-colors duration-200 disabled:opacity-50 disabled:hover:bg-transparent disabled:hover:text-muted-foreground"
      >
        <span className="text-lg font-medium">←</span>
      </button>
      <span className="px-3" data-testid="page-info">
        {pagination.pageNumber} / {pagination.totalPages}
      </span>
      <button
        onClick={() => onPageChange(pagination.pageNumber + 1)}
        disabled={!pagination.hasNextPage}
        data-testid="next-page"
        className="px-2 py-1 rounded border border-border hover:bg-accent hover:text-accent-foreground transition-colors duration-200 disabled:opacity-50 disabled:hover:bg-transparent disabled:hover:text-muted-foreground"
      >
        <span className="text-lg font-medium">→</span>
      </button>
    </div>
  );
}
