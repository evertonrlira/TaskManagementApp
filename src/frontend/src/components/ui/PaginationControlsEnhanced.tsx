import type { PaginationInfo } from '../../lib/types';

interface PaginationControlsEnhancedProps {
  pagination: PaginationInfo;
  onPageChange: (page: number) => void;
}

export function PaginationControlsEnhanced({ pagination, onPageChange }: PaginationControlsEnhancedProps) {
  return (
    <div className="flex justify-center gap-2" data-testid="pagination-enhanced">
      <button
        onClick={() => onPageChange(pagination.pageNumber - 1)}
        disabled={!pagination.hasPreviousPage}
        data-testid="prev-page"
        className="px-3 py-1 text-sm rounded border border-input bg-background hover:bg-accent disabled:opacity-50"
      >
        Previous
      </button>
      <span className="px-3 py-1 text-sm" data-testid="page-info">
        Page {pagination.pageNumber} of {pagination.totalPages}
      </span>
      <button
        onClick={() => onPageChange(pagination.pageNumber + 1)}
        disabled={!pagination.hasNextPage}
        data-testid="next-page"
        className="px-3 py-1 text-sm rounded border border-input bg-background hover:bg-accent disabled:opacity-50"
      >
        Next
      </button>
    </div>
  );
}
