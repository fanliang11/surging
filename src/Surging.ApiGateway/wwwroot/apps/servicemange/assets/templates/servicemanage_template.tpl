       {{each Entity}}
	<tr>
                            <td class="center">
								  {{if ($index+1)%2==1 }}
                                  <span class="badge badge-grey ">${$index+1}</span>
                                  {{else}}
                                  <span class="badge   badge-success  ">${$index+1}</span>
                                  {{/if}}
                            </td>
                            <td class="center"> {{= [Address.Ip,Address.Port].join(":")}}</td>
                            <td class="center">已启动</td>
                            <td class="center">
							      {{if IsHealth==true }}
								<span class="label label-success arrowed-in arrowed-in-right">正常</span>
								{{else}}
										<span class="label label-danger arrowed">异常</span>
								{{/if}}
							</td>
                            <td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                                                   <a href="/ServiceManage/ServiceDescriptor?address={{= [Address.Ip,Address.Port].join(":")}}" nodemenu="" class="grey editRegion" data-pjax=".page-content"><i class="icon-lightbulb  bigger-120"></i>查看元数据</a>
												     <a href="/ServiceManage/FaultTolerant?address={{= [Address.Ip,Address.Port].join(":")}}" nodemenu="" class="grey editRegion" data-pjax=".page-content"><i class="icon-lightbulb  bigger-120"></i>查看容错规则</a>
                                                   <a href="javascript:void(0);" class="red delRegion"><i class="icon-trash bigger-120"></i>禁用</a>

                             </div></td>
                           
                        </tr>
						  {{/each}}