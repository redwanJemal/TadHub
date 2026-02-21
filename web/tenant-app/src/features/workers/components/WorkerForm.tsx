import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import { 
  useWorker, 
  useCreateWorker, 
  useUpdateWorker,
} from '../hooks/use-workers';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Checkbox } from '@/shared/components/ui/checkbox';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/shared/components/ui/form';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { JobCategorySelect, NationalitySelect } from '@/features/reference-data';
import { 
  RELIGIONS, 
  MARITAL_STATUSES, 
  EDUCATION_LEVELS,
  type Gender 
} from '../types';

// Form validation schema
const workerFormSchema = z.object({
  passportNumber: z.string().min(1, 'Passport number is required'),
  emiratesId: z.string().optional(),
  fullNameEn: z.string().min(1, 'English name is required'),
  fullNameAr: z.string().min(1, 'Arabic name is required'),
  nationality: z.string().min(1, 'Nationality is required'),
  dateOfBirth: z.string().min(1, 'Date of birth is required'),
  gender: z.enum(['Male', 'Female']),
  religion: z.string().min(1, 'Religion is required'),
  maritalStatus: z.string().min(1, 'Marital status is required'),
  numberOfChildren: z.number().optional(),
  education: z.string().min(1, 'Education is required'),
  yearsOfExperience: z.number().optional(),
  jobCategoryId: z.string().min(1, 'Job category is required'),
  monthlyBaseSalary: z.number().min(0, 'Salary must be positive'),
  isAvailableForFlexible: z.boolean(),
  notes: z.string().optional(),
});

type WorkerFormValues = z.infer<typeof workerFormSchema>;

export function WorkerForm() {
  const { id } = useParams<{ id: string }>();
  const isEditMode = !!id;
  const { t } = useTranslation('workers');
  const navigate = useNavigate();

  const { data: worker, isLoading: isLoadingWorker } = useWorker(id || '', ['jobCategory']);
  const createWorker = useCreateWorker();
  const updateWorker = useUpdateWorker();

  const form = useForm<WorkerFormValues>({
    resolver: zodResolver(workerFormSchema),
    defaultValues: {
      passportNumber: '',
      emiratesId: '',
      fullNameEn: '',
      fullNameAr: '',
      nationality: '',
      dateOfBirth: '',
      gender: 'Female' as Gender,
      religion: '',
      maritalStatus: '',
      numberOfChildren: 0,
      education: '',
      yearsOfExperience: 0,
      jobCategoryId: '',
      monthlyBaseSalary: 0,
      isAvailableForFlexible: false,
      notes: '',
    },
  });

  // Populate form when editing
  useEffect(() => {
    if (isEditMode && worker) {
      form.reset({
        passportNumber: worker.passportNumber,
        emiratesId: worker.emiratesId || '',
        fullNameEn: worker.fullNameEn,
        fullNameAr: worker.fullNameAr,
        nationality: worker.nationality,
        dateOfBirth: worker.dateOfBirth,
        gender: worker.gender,
        religion: worker.religion,
        maritalStatus: worker.maritalStatus,
        numberOfChildren: worker.numberOfChildren || 0,
        education: worker.education,
        yearsOfExperience: worker.yearsOfExperience || 0,
        jobCategoryId: worker.jobCategory?.id || '',
        monthlyBaseSalary: worker.monthlyBaseSalary,
        isAvailableForFlexible: worker.isAvailableForFlexible,
        notes: worker.notes || '',
      });
    }
  }, [isEditMode, worker, form]);

  const onSubmit = async (data: WorkerFormValues) => {
    try {
      if (isEditMode) {
        await updateWorker.mutateAsync({
          id: id!,
          data: {
            fullNameEn: data.fullNameEn,
            fullNameAr: data.fullNameAr,
            emiratesId: data.emiratesId || undefined,
            religion: data.religion,
            maritalStatus: data.maritalStatus,
            numberOfChildren: data.numberOfChildren,
            education: data.education,
            yearsOfExperience: data.yearsOfExperience,
            jobCategoryId: data.jobCategoryId,
            monthlyBaseSalary: data.monthlyBaseSalary,
            isAvailableForFlexible: data.isAvailableForFlexible,
            notes: data.notes || undefined,
          },
        });
        navigate(`/workers/${id}`);
      } else {
        const result = await createWorker.mutateAsync({
          passportNumber: data.passportNumber,
          emiratesId: data.emiratesId || undefined,
          fullNameEn: data.fullNameEn,
          fullNameAr: data.fullNameAr,
          nationality: data.nationality,
          dateOfBirth: data.dateOfBirth,
          gender: data.gender,
          religion: data.religion,
          maritalStatus: data.maritalStatus,
          numberOfChildren: data.numberOfChildren,
          education: data.education,
          yearsOfExperience: data.yearsOfExperience,
          jobCategoryId: data.jobCategoryId,
          monthlyBaseSalary: data.monthlyBaseSalary,
          isAvailableForFlexible: data.isAvailableForFlexible,
          notes: data.notes || undefined,
        });
        navigate(`/workers/${result.id}`);
      }
    } catch (error) {
      console.error('Failed to save worker:', error);
    }
  };

  const isLoading = isEditMode && isLoadingWorker;
  const isSaving = createWorker.isPending || updateWorker.isPending;

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10" />
          <Skeleton className="h-8 w-48" />
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="space-y-4">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="space-y-2">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-10 w-full" />
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-2xl font-bold">
            {isEditMode ? t('form.title.edit') : t('form.title.create')}
          </h1>
        </div>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          {/* Personal Information */}
          <Card>
            <CardHeader>
              <CardTitle>{t('form.sections.personal')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-6 md:grid-cols-2">
              <FormField
                control={form.control}
                name="passportNumber"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.passportNumber')}</FormLabel>
                    <FormControl>
                      <Input 
                        {...field} 
                        disabled={isEditMode}
                        placeholder={t('form.placeholders.passportNumber')} 
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="emiratesId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.emiratesId')}</FormLabel>
                    <FormControl>
                      <Input {...field} placeholder={t('form.placeholders.emiratesId')} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="fullNameEn"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.fullNameEn')}</FormLabel>
                    <FormControl>
                      <Input {...field} placeholder={t('form.placeholders.fullNameEn')} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="fullNameAr"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.fullNameAr')}</FormLabel>
                    <FormControl>
                      <Input {...field} dir="rtl" placeholder={t('form.placeholders.fullNameAr')} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="nationality"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.nationality')}</FormLabel>
                    <FormControl>
                      <NationalitySelect
                        value={field.value}
                        onChange={field.onChange}
                        disabled={isEditMode}
                        placeholder={t('form.placeholders.selectNationality')}
                        valueType="name"
                        commonOnly
                        error={!!form.formState.errors.nationality}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="dateOfBirth"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.dateOfBirth')}</FormLabel>
                    <FormControl>
                      <Input {...field} type="date" disabled={isEditMode} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="gender"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.gender')}</FormLabel>
                    <Select 
                      value={field.value} 
                      onValueChange={field.onChange}
                      disabled={isEditMode}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={t('form.placeholders.selectGender')} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="Female">{t('gender.Female')}</SelectItem>
                        <SelectItem value="Male">{t('gender.Male')}</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="religion"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.religion')}</FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={t('form.placeholders.selectReligion')} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {RELIGIONS.map((religion) => (
                          <SelectItem key={religion} value={religion}>{religion}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="maritalStatus"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.maritalStatus')}</FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={t('form.placeholders.selectMaritalStatus')} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {MARITAL_STATUSES.map((status) => (
                          <SelectItem key={status} value={status}>{status}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="numberOfChildren"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.numberOfChildren')}</FormLabel>
                    <FormControl>
                      <Input 
                        {...field} 
                        type="number" 
                        min={0}
                        onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="education"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.education')}</FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={t('form.placeholders.selectEducation')} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {EDUCATION_LEVELS.map((level) => (
                          <SelectItem key={level} value={level}>{level}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </CardContent>
          </Card>

          {/* Employment Details */}
          <Card>
            <CardHeader>
              <CardTitle>{t('form.sections.employment')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-6 md:grid-cols-2">
              <FormField
                control={form.control}
                name="jobCategoryId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.jobCategory')}</FormLabel>
                    <FormControl>
                      <JobCategorySelect
                        value={field.value}
                        onChange={field.onChange}
                        placeholder={t('form.placeholders.selectJobCategory')}
                        showCode
                        error={!!form.formState.errors.jobCategoryId}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="yearsOfExperience"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.yearsOfExperience')}</FormLabel>
                    <FormControl>
                      <Input 
                        {...field} 
                        type="number" 
                        min={0}
                        onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="monthlyBaseSalary"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('form.fields.monthlyBaseSalary')}</FormLabel>
                    <FormControl>
                      <Input 
                        {...field} 
                        type="number" 
                        min={0}
                        onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="isAvailableForFlexible"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                    <FormControl>
                      <Checkbox
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                    <div className="space-y-1 leading-none">
                      <FormLabel>{t('form.fields.isAvailableForFlexible')}</FormLabel>
                      <FormDescription>
                        Worker can be assigned to flexible/temporary contracts
                      </FormDescription>
                    </div>
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="notes"
                render={({ field }) => (
                  <FormItem className="md:col-span-2">
                    <FormLabel>{t('form.fields.notes')}</FormLabel>
                    <FormControl>
                      <Input {...field} placeholder={t('form.placeholders.notes')} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </CardContent>
          </Card>

          {/* Actions */}
          <div className="flex justify-end gap-4">
            <Button type="button" variant="outline" onClick={() => navigate(-1)}>
              {t('common:cancel')}
            </Button>
            <Button type="submit" disabled={isSaving}>
              {isSaving ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  {t('common:saving')}
                </>
              ) : (
                <>
                  <Save className="mr-2 h-4 w-4" />
                  {t('common:save')}
                </>
              )}
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
}
