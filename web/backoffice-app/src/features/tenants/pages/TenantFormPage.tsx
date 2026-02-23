import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Loader2 } from 'lucide-react';
import { useTenant, useCreateTenant, useUpdateTenant } from '../hooks';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Textarea } from '@/shared/components/ui/textarea';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/shared/components/ui/form';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

// Base tenant fields shared between create and edit
const baseTenantFields = {
  name: z.string().min(2, 'Name must be at least 2 characters').max(200, 'Name is too long'),
  slug: z
    .string()
    .regex(/^[a-z0-9-]+$/, 'Slug can only contain lowercase letters, numbers, and hyphens')
    .min(2, 'Slug must be at least 2 characters')
    .max(50, 'Slug is too long')
    .optional()
    .or(z.literal('')),
  description: z.string().max(1000, 'Description is too long').optional().or(z.literal('')),
  website: z.string().url('Invalid URL').optional().or(z.literal('')),
  logoUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
};

// Create form includes owner fields
const createTenantSchema = z.object({
  ...baseTenantFields,
  ownerEmail: z.string().email('Invalid email address'),
  ownerPassword: z.string().min(8, 'Password must be at least 8 characters'),
  ownerFirstName: z.string().min(1, 'First name is required').max(100),
  ownerLastName: z.string().min(1, 'Last name is required').max(100),
});

// Edit form — no owner fields
const editTenantSchema = z.object(baseTenantFields);

type CreateTenantFormValues = z.infer<typeof createTenantSchema>;

export function TenantFormPage() {
  const navigate = useNavigate();
  const { tenantId } = useParams<{ tenantId: string }>();
  const isEditing = !!tenantId;

  const { data: tenant, isLoading: isTenantLoading } = useTenant(tenantId ?? '');
  const createMutation = useCreateTenant();
  const updateMutation = useUpdateTenant();

  const form = useForm<CreateTenantFormValues>({
    resolver: zodResolver(isEditing ? editTenantSchema : createTenantSchema),
    defaultValues: {
      name: '',
      slug: '',
      description: '',
      website: '',
      logoUrl: '',
      ...(!isEditing && {
        ownerEmail: '',
        ownerPassword: '',
        ownerFirstName: '',
        ownerLastName: '',
      }),
    },
  });

  // Populate form when editing
  useEffect(() => {
    if (tenant) {
      form.reset({
        name: tenant.name,
        slug: tenant.slug,
        description: tenant.description ?? '',
        website: tenant.website ?? '',
        logoUrl: tenant.logoUrl ?? '',
      });
    }
  }, [tenant, form]);

  const onSubmit = async (values: CreateTenantFormValues) => {
    try {
      if (isEditing && tenantId) {
        const data = {
          name: values.name,
          slug: values.slug || undefined,
          description: values.description || undefined,
          website: values.website || undefined,
          logoUrl: values.logoUrl || undefined,
        };
        await updateMutation.mutateAsync({ tenantId, data });
        navigate(`/tenants/${tenantId}`);
      } else {
        const data = {
          name: values.name,
          slug: values.slug || undefined,
          description: values.description || undefined,
          website: values.website || undefined,
          logoUrl: values.logoUrl || undefined,
          ownerEmail: values.ownerEmail,
          ownerPassword: values.ownerPassword,
          ownerFirstName: values.ownerFirstName,
          ownerLastName: values.ownerLastName,
        };
        const newTenant = await createMutation.mutateAsync(data);
        navigate(`/tenants/${newTenant.id}`);
      }
    } catch (error) {
      console.error('Failed to save tenant:', error);
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  // Loading state for edit mode
  if (isEditing && isTenantLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10" />
          <div className="space-y-2">
            <Skeleton className="h-6 w-[200px]" />
            <Skeleton className="h-4 w-[150px]" />
          </div>
        </div>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-[150px]" />
            <Skeleton className="h-4 w-[250px]" />
          </CardHeader>
          <CardContent className="space-y-6">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="space-y-2">
                <Skeleton className="h-4 w-[100px]" />
                <Skeleton className="h-10 w-full" />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/tenants')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {isEditing ? 'Edit Tenant' : 'Create Tenant'}
          </h1>
          <p className="text-muted-foreground">
            {isEditing ? 'Update tenant information' : 'Add a new agency or organization'}
          </p>
        </div>
      </div>

      {/* Form */}
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>
                The core details about this tenant organization
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Name *</FormLabel>
                    <FormControl>
                      <Input placeholder="Tadbeer Agency" {...field} />
                    </FormControl>
                    <FormDescription>
                      The display name of the organization
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="slug"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Slug</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="tadbeer-agency"
                        {...field}
                        disabled={isEditing}
                      />
                    </FormControl>
                    <FormDescription>
                      URL-friendly identifier. Auto-generated if left empty.
                      {isEditing && ' Cannot be changed after creation.'}
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Description</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="A brief description of the organization..."
                        className="resize-none"
                        rows={3}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Optional description for internal reference
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Branding & Links</CardTitle>
              <CardDescription>
                Optional branding and external links
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <FormField
                control={form.control}
                name="logoUrl"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Logo URL</FormLabel>
                    <FormControl>
                      <Input placeholder="https://example.com/logo.png" {...field} />
                    </FormControl>
                    <FormDescription>
                      URL to the organization's logo image
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="website"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Website</FormLabel>
                    <FormControl>
                      <Input placeholder="https://example.com" {...field} />
                    </FormControl>
                    <FormDescription>
                      The organization's website URL
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </CardContent>
          </Card>

          {/* Owner Information — only shown when creating */}
          {!isEditing && (
            <Card>
              <CardHeader>
                <CardTitle>Owner Account</CardTitle>
                <CardDescription>
                  The owner user who will manage this tenant. A new account will be created with these credentials.
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="ownerFirstName"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>First Name *</FormLabel>
                        <FormControl>
                          <Input placeholder="John" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="ownerLastName"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Last Name *</FormLabel>
                        <FormControl>
                          <Input placeholder="Doe" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="ownerEmail"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Email *</FormLabel>
                      <FormControl>
                        <Input type="email" placeholder="owner@example.com" {...field} />
                      </FormControl>
                      <FormDescription>
                        This will be used as the login email for the owner account
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="ownerPassword"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Password *</FormLabel>
                      <FormControl>
                        <Input type="password" placeholder="Minimum 8 characters" {...field} />
                      </FormControl>
                      <FormDescription>
                        The owner can change this after their first login
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </CardContent>
            </Card>
          )}

          {/* Form Actions */}
          <div className="flex items-center gap-4">
            <Button type="submit" disabled={isPending}>
              {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isEditing ? 'Save Changes' : 'Create Tenant'}
            </Button>
            <Button type="button" variant="outline" onClick={() => navigate('/tenants')}>
              Cancel
            </Button>
          </div>

          {/* Error Display */}
          {(createMutation.error || updateMutation.error) && (
            <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4">
              <p className="text-sm text-destructive">
                {(createMutation.error || updateMutation.error)?.message ?? 'An error occurred'}
              </p>
            </div>
          )}
        </form>
      </Form>
    </div>
  );
}
