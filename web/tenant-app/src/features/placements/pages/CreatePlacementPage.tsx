import { useState } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import { useCandidates } from '@/features/candidates/hooks';
import { useClients } from '@/features/clients/hooks';
import { useCreatePlacement } from '../hooks';

export function CreatePlacementPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const createMutation = useCreatePlacement();

  // Pre-fill from URL params
  const prefilledCandidateId = searchParams.get('candidateId') || '';
  const prefilledCandidateName = searchParams.get('candidateName') || '';

  // Form state
  const [candidateId, setCandidateId] = useState(prefilledCandidateId);
  const [clientId, setClientId] = useState('');
  const [bookingNotes, setBookingNotes] = useState('');

  // Candidate search
  const [candidateSearch, setCandidateSearch] = useState(prefilledCandidateName);
  const [showCandidateDropdown, setShowCandidateDropdown] = useState(false);
  const { data: candidatesData } = useCandidates({
    pageSize: 50,
    search: candidateSearch || undefined,
    'filter[status]': 'Approved',
  });

  // Client search
  const [clientSearch, setClientSearch] = useState('');
  const [showClientDropdown, setShowClientDropdown] = useState(false);
  const { data: clientsData } = useClients({
    pageSize: 50,
    search: clientSearch || undefined,
    'filter[isActive]': 'true',
  });

  const canSubmit = candidateId && clientId;

  const handleSubmit = async () => {
    if (!canSubmit) return;
    await createMutation.mutateAsync({
      candidateId,
      clientId,
      bookingNotes: bookingNotes.trim() || undefined,
    });
    navigate('/placements');
  };

  return (
    <div className="space-y-6 p-6">
      <div>
        <Link to="/placements" className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" />
          Back to Pipeline
        </Link>
        <h1 className="text-2xl font-semibold">Book Candidate for Client</h1>
        <p className="text-sm text-muted-foreground">
          Create a new placement to start the deployment pipeline
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Candidate Selection */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Select Candidate</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="relative">
              <Label>Candidate (Approved only)</Label>
              <Input
                value={candidateSearch}
                onChange={(e) => {
                  setCandidateSearch(e.target.value);
                  setCandidateId('');
                  setShowCandidateDropdown(true);
                }}
                onFocus={() => setShowCandidateDropdown(true)}
                placeholder="Search candidates..."
              />
              {showCandidateDropdown && candidatesData?.items && candidatesData.items.length > 0 && (
                <div className="absolute z-10 mt-1 max-h-48 w-full overflow-y-auto rounded-md border bg-popover shadow-md">
                  {candidatesData.items.map((c) => (
                    <button
                      key={c.id}
                      type="button"
                      className="flex w-full items-center gap-2 px-3 py-2 text-sm hover:bg-muted"
                      onClick={() => {
                        setCandidateId(c.id);
                        setCandidateSearch(c.fullNameEn);
                        setShowCandidateDropdown(false);
                      }}
                    >
                      {c.photoUrl ? (
                        <img src={c.photoUrl} className="h-6 w-6 rounded-full object-cover" alt="" />
                      ) : (
                        <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary/10 text-xs font-medium text-primary">
                          {c.fullNameEn[0]}
                        </div>
                      )}
                      <span>{c.fullNameEn}</span>
                      <span className="ml-auto text-xs text-muted-foreground">{c.nationality}</span>
                    </button>
                  ))}
                </div>
              )}
            </div>
            {candidateId && (
              <p className="text-xs text-green-600">Candidate selected</p>
            )}
          </CardContent>
        </Card>

        {/* Client Selection */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Select Client</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="relative">
              <Label>Client (Active only)</Label>
              <Input
                value={clientSearch}
                onChange={(e) => {
                  setClientSearch(e.target.value);
                  setClientId('');
                  setShowClientDropdown(true);
                }}
                onFocus={() => setShowClientDropdown(true)}
                placeholder="Search clients..."
              />
              {showClientDropdown && clientsData?.items && clientsData.items.length > 0 && (
                <div className="absolute z-10 mt-1 max-h-48 w-full overflow-y-auto rounded-md border bg-popover shadow-md">
                  {clientsData.items.map((c) => (
                    <button
                      key={c.id}
                      type="button"
                      className="flex w-full items-center gap-2 px-3 py-2 text-sm hover:bg-muted"
                      onClick={() => {
                        setClientId(c.id);
                        setClientSearch(c.nameEn);
                        setShowClientDropdown(false);
                      }}
                    >
                      <span>{c.nameEn}</span>
                      {c.city && <span className="ml-auto text-xs text-muted-foreground">{c.city}</span>}
                    </button>
                  ))}
                </div>
              )}
            </div>
            {clientId && (
              <p className="text-xs text-green-600">Client selected</p>
            )}
          </CardContent>
        </Card>

        {/* Notes */}
        <Card className="md:col-span-2">
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Booking Notes</CardTitle>
          </CardHeader>
          <CardContent>
            <textarea
              className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              value={bookingNotes}
              onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setBookingNotes(e.target.value)}
              placeholder="Any notes about this booking..."
              rows={3}
            />
          </CardContent>
        </Card>
      </div>

      <div className="flex justify-end gap-3">
        <Button variant="outline" onClick={() => navigate('/placements')}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          disabled={!canSubmit || createMutation.isPending}
        >
          {createMutation.isPending ? 'Creating...' : 'Create Placement'}
        </Button>
      </div>
    </div>
  );
}
