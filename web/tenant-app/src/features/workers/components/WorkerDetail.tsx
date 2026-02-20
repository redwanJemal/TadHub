import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { 
  ArrowLeft, 
  Edit, 
  Trash2, 
  MoreHorizontal, 
  MapPin, 
  Calendar, 
  Briefcase,
  DollarSign,
  Globe,
  GraduationCap,
  Heart,
  Users,
  Clock
} from 'lucide-react';
import { 
  useWorker, 
  useWorkerHistory, 
  useValidTransitions,
  useDeleteWorker,
  useTransitionWorker 
} from '../hooks/use-workers';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
import { Avatar, AvatarFallback, AvatarImage } from '@/shared/components/ui/avatar';
import { Skeleton } from '@/shared/components/ui/skeleton';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from '@/shared/components/ui/dropdown-menu';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Input } from '@/shared/components/ui/input';
import { STATUS_COLORS, type WorkerStatus } from '../types';

export function WorkerDetail() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation('workers');
  const navigate = useNavigate();

  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showTransitionDialog, setShowTransitionDialog] = useState(false);
  const [selectedTransition, setSelectedTransition] = useState<string>('');
  const [transitionReason, setTransitionReason] = useState('');

  const { data: worker, isLoading, error } = useWorker(id!, ['skills', 'languages', 'jobCategory']);
  const { data: history } = useWorkerHistory(id!);
  const { data: validTransitions } = useValidTransitions(id!);
  const deleteWorker = useDeleteWorker();
  const transitionWorker = useTransitionWorker();

  const handleDelete = async () => {
    if (id) {
      await deleteWorker.mutateAsync(id);
      navigate('/workers');
    }
  };

  const handleTransition = async () => {
    if (id && selectedTransition) {
      await transitionWorker.mutateAsync({
        id,
        data: {
          targetState: selectedTransition as WorkerStatus,
          reason: transitionReason || undefined,
        },
      });
      setShowTransitionDialog(false);
      setSelectedTransition('');
      setTransitionReason('');
    }
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10" />
          <Skeleton className="h-8 w-48" />
        </div>
        <div className="grid gap-6 md:grid-cols-3">
          <Card className="md:col-span-1">
            <CardContent className="pt-6">
              <div className="flex flex-col items-center gap-4">
                <Skeleton className="h-24 w-24 rounded-full" />
                <Skeleton className="h-6 w-32" />
                <Skeleton className="h-4 w-24" />
              </div>
            </CardContent>
          </Card>
          <Card className="md:col-span-2">
            <CardContent className="pt-6">
              <div className="space-y-4">
                {Array.from({ length: 6 }).map((_, i) => (
                  <div key={i} className="flex justify-between">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-4 w-32" />
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  if (error || !worker) {
    return (
      <div className="p-8 text-center">
        <p className="text-red-500">{t('error.loadingFailed')}</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate('/workers')}>
          {t('common:back')}
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/workers')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold">{worker.fullNameEn}</h1>
            <p className="text-muted-foreground" dir="rtl">{worker.fullNameAr}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {validTransitions && validTransitions.length > 0 && (
            <Button variant="outline" onClick={() => setShowTransitionDialog(true)}>
              {t('actions.transition')}
            </Button>
          )}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" size="icon">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => navigate(`/workers/${id}/edit`)}>
                <Edit className="mr-2 h-4 w-4" />
                {t('actions.edit')}
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                className="text-red-600"
                onClick={() => setShowDeleteDialog(true)}
              >
                <Trash2 className="mr-2 h-4 w-4" />
                {t('actions.delete')}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>

      {/* Content */}
      <div className="grid gap-6 md:grid-cols-3">
        {/* Profile Card */}
        <Card className="md:col-span-1">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center gap-4">
              <Avatar className="h-24 w-24">
                <AvatarImage src={worker.photoUrl} alt={worker.fullNameEn} />
                <AvatarFallback className="text-2xl">{getInitials(worker.fullNameEn)}</AvatarFallback>
              </Avatar>
              <div className="text-center">
                <p className="font-semibold">{worker.fullNameEn}</p>
                <p className="text-sm text-muted-foreground">{worker.cvSerial}</p>
              </div>
              <Badge className={STATUS_COLORS[worker.currentStatus]}>
                {t(`status.${worker.currentStatus}`)}
              </Badge>
              <div className="w-full pt-4 space-y-3">
                <div className="flex items-center gap-2 text-sm">
                  <MapPin className="h-4 w-4 text-muted-foreground" />
                  <span>{t(`passportLocation.${worker.passportLocation}`)}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span>{worker.age} {t('detail.yearsOld')}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <DollarSign className="h-4 w-4 text-muted-foreground" />
                  <span>AED {worker.monthlyBaseSalary.toLocaleString()}/month</span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Details Tabs */}
        <Card className="md:col-span-2">
          <Tabs defaultValue="overview">
            <CardHeader>
              <TabsList>
                <TabsTrigger value="overview">{t('detail.overview')}</TabsTrigger>
                <TabsTrigger value="history">{t('detail.history')}</TabsTrigger>
                <TabsTrigger value="documents">{t('detail.documents')}</TabsTrigger>
              </TabsList>
            </CardHeader>
            <CardContent>
              <TabsContent value="overview" className="space-y-6">
                {/* Personal Info */}
                <div>
                  <h3 className="font-semibold mb-3">{t('form.sections.personal')}</h3>
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div className="flex items-center gap-2">
                      <Globe className="h-4 w-4 text-muted-foreground" />
                      <span className="text-muted-foreground">{t('form.fields.nationality')}:</span>
                      <span>{worker.nationality}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Heart className="h-4 w-4 text-muted-foreground" />
                      <span className="text-muted-foreground">{t('form.fields.religion')}:</span>
                      <span>{worker.religion}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Users className="h-4 w-4 text-muted-foreground" />
                      <span className="text-muted-foreground">{t('form.fields.maritalStatus')}:</span>
                      <span>{worker.maritalStatus}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <GraduationCap className="h-4 w-4 text-muted-foreground" />
                      <span className="text-muted-foreground">{t('form.fields.education')}:</span>
                      <span>{worker.education}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Briefcase className="h-4 w-4 text-muted-foreground" />
                      <span className="text-muted-foreground">{t('form.fields.jobCategory')}:</span>
                      <span>{worker.jobCategory?.name || '-'}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Clock className="h-4 w-4 text-muted-foreground" />
                      <span className="text-muted-foreground">{t('form.fields.yearsOfExperience')}:</span>
                      <span>{worker.yearsOfExperience || 0} years</span>
                    </div>
                  </div>
                </div>

                {/* Skills */}
                <div>
                  <h3 className="font-semibold mb-3">{t('detail.skills')}</h3>
                  {worker.skills && worker.skills.length > 0 ? (
                    <div className="flex flex-wrap gap-2">
                      {worker.skills.map((skill) => (
                        <Badge key={skill.id} variant="secondary">
                          {skill.skillName} ({skill.rating}%)
                        </Badge>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">{t('detail.noSkills')}</p>
                  )}
                </div>

                {/* Languages */}
                <div>
                  <h3 className="font-semibold mb-3">{t('detail.languages')}</h3>
                  {worker.languages && worker.languages.length > 0 ? (
                    <div className="flex flex-wrap gap-2">
                      {worker.languages.map((lang) => (
                        <Badge key={lang.id} variant="outline">
                          {lang.language} - {t(`proficiency.${lang.proficiency}`)}
                        </Badge>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">{t('detail.noLanguages')}</p>
                  )}
                </div>

                {/* Notes */}
                {worker.notes && (
                  <div>
                    <h3 className="font-semibold mb-3">{t('form.fields.notes')}</h3>
                    <p className="text-sm text-muted-foreground">{worker.notes}</p>
                  </div>
                )}
              </TabsContent>

              <TabsContent value="history">
                <div className="space-y-4">
                  <h3 className="font-semibold">{t('detail.stateHistory')}</h3>
                  {history && history.items.length > 0 ? (
                    <div className="space-y-3">
                      {history.items.map((entry) => (
                        <div
                          key={entry.id}
                          className="flex items-start gap-4 p-3 rounded-lg bg-muted/50"
                        >
                          <div className="flex-1">
                            <div className="flex items-center gap-2">
                              <Badge className={STATUS_COLORS[entry.fromStatus]} variant="outline">
                                {t(`status.${entry.fromStatus}`)}
                              </Badge>
                              <span>→</span>
                              <Badge className={STATUS_COLORS[entry.toStatus]}>
                                {t(`status.${entry.toStatus}`)}
                              </Badge>
                            </div>
                            {entry.reason && (
                              <p className="text-sm text-muted-foreground mt-1">{entry.reason}</p>
                            )}
                          </div>
                          <span className="text-xs text-muted-foreground">
                            {new Date(entry.occurredAt).toLocaleDateString()}
                          </span>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">{t('detail.noHistory')}</p>
                  )}
                </div>
              </TabsContent>

              <TabsContent value="documents">
                <div className="text-center py-8">
                  <p className="text-muted-foreground">Documents feature coming soon...</p>
                </div>
              </TabsContent>
            </CardContent>
          </Tabs>
        </Card>
      </div>

      {/* Delete Dialog */}
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('deleteDialog.title')}</AlertDialogTitle>
            <AlertDialogDescription>{t('deleteDialog.description')}</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-red-600 hover:bg-red-700"
            >
              {t('common:delete')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Transition Dialog */}
      <Dialog open={showTransitionDialog} onOpenChange={setShowTransitionDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t('transition.title')}</DialogTitle>
            <DialogDescription>
              {t('transition.currentStatus')}: {t(`status.${worker.currentStatus}`)}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">{t('transition.newStatus')}</label>
              <Select value={selectedTransition} onValueChange={setSelectedTransition}>
                <SelectTrigger>
                  <SelectValue placeholder={t('transition.newStatus')} />
                </SelectTrigger>
                <SelectContent>
                  {validTransitions?.map((status) => (
                    <SelectItem key={status} value={status}>
                      {t(`status.${status}`)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">{t('transition.reason')}</label>
              <Input
                value={transitionReason}
                onChange={(e) => setTransitionReason(e.target.value)}
                placeholder={t('transition.reasonPlaceholder')}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowTransitionDialog(false)}>
              {t('common:cancel')}
            </Button>
            <Button onClick={handleTransition} disabled={!selectedTransition}>
              {t('transition.confirm')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
